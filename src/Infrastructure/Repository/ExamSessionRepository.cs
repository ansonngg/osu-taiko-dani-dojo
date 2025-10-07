using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Infrastructure.Model;
using Supabase.Postgrest;
using Client = Supabase.Client;

namespace OsuTaikoDaniDojo.Infrastructure.Repository;

public class ExamSessionRepository(Client database) : IExamSessionRepository
{
    private readonly Client _database = database;

    public async Task<int> CreateAsync(int osuId, int grade)
    {
        var response = await _database
            .From<ExamSession>()
            .Insert(
                new ExamSession { OsuId = osuId, Grade = grade },
                new QueryOptions { Returning = QueryOptions.ReturnType.Representation });

        return response.Model?.Id ?? throw new NullReferenceException("Returned exam session is null.");
    }

    public async Task ProceedToNextStageAsync(int examSessionId)
    {
        await _database.Rpc("increment_exam_session_stage", new { exam_session_id = examSessionId });
    }

    public async Task SetCompletedAsync(int examSessionId)
    {
        await _EndAsync(examSessionId, "completed");
    }

    public async Task SetTimeOutAsync(int examSessionId)
    {
        await _EndAsync(examSessionId, "time_out");
    }

    public async Task SetNoResponseAsync(int examSessionId)
    {
        await _EndAsync(examSessionId, "no_response");
    }

    public async Task TerminateAsync(int examSessionId)
    {
        await _EndAsync(examSessionId, "terminated");
    }

    private async Task _EndAsync(int examSessionId, string finalStatus)
    {
        await _database
            .From<ExamSession>()
            .Where(x => x.Id == examSessionId)
            .Set(x => x.EndedAt!, DateTime.UtcNow)
            .Set(x => x.Status, finalStatus)
            .Update();
    }
}

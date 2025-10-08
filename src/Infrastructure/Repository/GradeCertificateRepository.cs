using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Infrastructure.Model;
using Supabase.Postgrest;
using Client = Supabase.Client;

namespace OsuTaikoDaniDojo.Infrastructure.Repository;

public class GradeCertificateRepository(Client database) : IGradeCertificateRepository
{
    private readonly Client _database = database;

    public async Task<int> CreateAsync(
        int userId,
        int grade,
        int passLevel,
        int[] greatCounts,
        int[] okCounts,
        int[] missCounts,
        int[] largeBonusCounts,
        int[] maxCombos,
        int[] hitCounts,
        int examSessionId)
    {
        var response = await _database
            .From<GradeCertificate>()
            .Insert(
                new GradeCertificate
                {
                    UserId = userId,
                    Grade = grade,
                    PassLevel = passLevel,
                    GreatCounts = greatCounts,
                    OkCounts = okCounts,
                    MissCounts = missCounts,
                    LargeBonusCounts = largeBonusCounts,
                    MaxCombos = maxCombos,
                    HitCounts = hitCounts,
                    ExamSessionId = examSessionId
                },
                new QueryOptions { Returning = QueryOptions.ReturnType.Representation });

        return response.Model?.Id ?? throw new NullReferenceException("Returned grade certificate is null.");
    }
}

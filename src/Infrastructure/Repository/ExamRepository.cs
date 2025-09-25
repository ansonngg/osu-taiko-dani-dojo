using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Infrastructure.Model;
using Supabase;

namespace OsuTaikoDaniDojo.Infrastructure.Repository;

public class ExamRepository(Client database) : IExamRepository
{
    private readonly Client _database = database;

    public async Task<ExamQuery?> GetExamByGradeAsync(int grade)
    {
        var response = await _database.From<Exam>().Where(x => x.Grade == grade).Single();

        return response != null
            ? new ExamQuery
            {
                BeatmapIds = response.BeatmapIds ?? [],
                SpecificGreatCounts = response.SpecificGreatCounts,
                SpecificOkCounts = response.SpecificOkCounts,
                SpecificMissCounts = response.SpecificMissCounts,
                SpecificLargeBonusCounts = response.SpecificLargeBonusCounts,
                SpecificMaxCombos = response.SpecificMaxCombos,
                SpecificHitCounts = response.SpecificHitCounts,
                GeneralGreatCount = response.GeneralGreatCount ?? 0,
                GeneralOkCount = response.GeneralOkCount ?? 0,
                GeneralMissCount = response.GeneralMissCount ?? 0,
                GeneralLargeBonusCount = response.GeneralLargeBonusCount ?? 0,
                GeneralMaxCombo = response.GeneralMaxCombo ?? 0,
                GeneralHitCount = response.GeneralHitCount ?? 0
            }
            : null;
    }
}

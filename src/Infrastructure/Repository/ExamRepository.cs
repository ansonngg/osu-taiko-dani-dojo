using Microsoft.Extensions.Caching.Memory;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Infrastructure.Model;
using Supabase;

namespace OsuTaikoDaniDojo.Infrastructure.Repository;

public class ExamRepository(Client database, IMemoryCache memoryCache) : IExamRepository
{
    private readonly Client _database = database;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<ExamQuery?> GetExamByGradeAsync(int grade)
    {
        if (_memoryCache.TryGetValue(_ExamQueryKey(grade), out ExamQuery? examQuery))
        {
            return examQuery;
        }

        var response = await _database.From<Exam>().Where(x => x.Grade == grade).Single();

        if (response == null)
        {
            return null;
        }

        examQuery = new ExamQuery
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
        };

        _memoryCache.Set(_ExamQueryKey(grade), examQuery);
        return examQuery;
    }

    private static string _ExamQueryKey(int grade) => $"Exam{grade}";
}

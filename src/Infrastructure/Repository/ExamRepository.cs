using Microsoft.Extensions.Caching.Memory;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Infrastructure.Model;
using Supabase;

namespace OsuTaikoDaniDojo.Infrastructure.Repository;

public class ExamRepository(Client database, IMemoryCache memoryCache) : IExamRepository
{
    private readonly Client _database = database;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<ExamQuery?> GetByGradeAsync(int grade)
    {
        if (_memoryCache.TryGetTyped(grade, out ExamQuery? examQuery))
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
            BeatmapIds = response.BeatmapIds,
            SpecificGreatCounts = response.SpecificGreatCounts,
            SpecificOkCounts = response.SpecificOkCounts,
            SpecificMissCounts = response.SpecificMissCounts,
            SpecificLargeBonusCounts = response.SpecificLargeBonusCounts,
            SpecificMaxCombos = response.SpecificMaxCombos,
            SpecificHitCounts = response.SpecificHitCounts,
            GeneralGreatCounts = response.GeneralGreatCounts,
            GeneralOkCounts = response.GeneralOkCounts,
            GeneralMissCounts = response.GeneralMissCounts,
            GeneralLargeBonusCounts = response.GeneralLargeBonusCounts,
            GeneralMaxCombos = response.GeneralMaxCombos,
            GeneralHitCounts = response.GeneralHitCounts
        };

        _memoryCache.SetTyped(grade, examQuery);
        return examQuery;
    }

    public async Task CreateAsync(int grade, ExamQuery examQuery)
    {
        await _database
            .From<Exam>()
            .Insert(
                new Exam
                {
                    Grade = grade,
                    BeatmapIds = examQuery.BeatmapIds,
                    SpecificGreatCounts = examQuery.SpecificGreatCounts,
                    SpecificOkCounts = examQuery.SpecificOkCounts,
                    SpecificMissCounts = examQuery.SpecificMissCounts,
                    SpecificLargeBonusCounts = examQuery.SpecificLargeBonusCounts,
                    SpecificMaxCombos = examQuery.SpecificMaxCombos,
                    SpecificHitCounts = examQuery.SpecificHitCounts,
                    GeneralGreatCounts = examQuery.GeneralGreatCounts,
                    GeneralOkCounts = examQuery.GeneralOkCounts,
                    GeneralMissCounts = examQuery.GeneralMissCounts,
                    GeneralLargeBonusCounts = examQuery.GeneralLargeBonusCounts,
                    GeneralMaxCombos = examQuery.GeneralMaxCombos,
                    GeneralHitCounts = examQuery.GeneralHitCounts
                });
    }
}

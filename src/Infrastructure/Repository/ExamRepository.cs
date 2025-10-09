using Microsoft.Extensions.Caching.Memory;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Infrastructure.Model;
using Supabase.Postgrest;
using Client = Supabase.Client;

namespace OsuTaikoDaniDojo.Infrastructure.Repository;

public class ExamRepository(Client database, IMemoryCache memoryCache) : IExamRepository
{
    private readonly Client _database = database;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task<ExamQuery[]> GetAllAsync()
    {
        var response = await _database.From<Exam>().Order(x => x.Grade, Constants.Ordering.Ascending).Get();

        return response != null
            ? response.Models
                .Select(
                    x =>
                    {
                        var examQuery = _ConstructExamQuery(x);
                        _memoryCache.SetTyped(x.Grade, examQuery);
                        return examQuery;
                    })
                .ToArray()
            : throw new NullReferenceException("Exam response is null.");
    }

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

        examQuery = _ConstructExamQuery(response);
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

        _memoryCache.RemoveTyped<ExamQuery>(grade);
    }

    private static ExamQuery _ConstructExamQuery(Exam exam)
    {
        return new ExamQuery
        {
            Grade = exam.Grade,
            BeatmapIds = exam.BeatmapIds,
            SpecificGreatCounts = exam.SpecificGreatCounts,
            SpecificOkCounts = exam.SpecificOkCounts,
            SpecificMissCounts = exam.SpecificMissCounts,
            SpecificLargeBonusCounts = exam.SpecificLargeBonusCounts,
            SpecificMaxCombos = exam.SpecificMaxCombos,
            SpecificHitCounts = exam.SpecificHitCounts,
            GeneralGreatCounts = exam.GeneralGreatCounts,
            GeneralOkCounts = exam.GeneralOkCounts,
            GeneralMissCounts = exam.GeneralMissCounts,
            GeneralLargeBonusCounts = exam.GeneralLargeBonusCounts,
            GeneralMaxCombos = exam.GeneralMaxCombos,
            GeneralHitCounts = exam.GeneralHitCounts
        };
    }
}

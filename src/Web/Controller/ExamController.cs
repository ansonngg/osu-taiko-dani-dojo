using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Web.Request;
using OsuTaikoDaniDojo.Web.Response;

namespace OsuTaikoDaniDojo.Web.Controller;

[ApiController]
[Route("api/[controller]")]
public class ExamController(IExamRepository examRepository) : ControllerBase
{
    private readonly IExamRepository _examRepository = examRepository;

    [HttpGet]
    public async Task<IActionResult> GetAllExams()
    {
        var examQueries = await _examRepository.GetAllAsync();
        return Ok(examQueries.Select(_ConstructExamResponse).ToArray());
    }

    [HttpGet("{grade:int}")]
    public async Task<IActionResult> GetExamByGrade(int grade)
    {
        var examQuery = await _examRepository.GetByGradeAsync(grade);

        if (examQuery == null)
        {
            return NotFound();
        }

        return Ok(_ConstructExamResponse(examQuery));
    }

    [Authorize(Roles = "Admin,Setter")]
    [HttpPost("{grade:int}")]
    public async Task<IActionResult> CreateExam(int grade, [FromBody] CreateExamRequest request)
    {
        await _examRepository.CreateAsync(
            grade,
            new ExamQuery
            {
                BeatmapIds = request.BeatmapIds,
                SpecificGreatCounts = request.SpecificGreatCounts,
                SpecificOkCounts = request.SpecificOkCounts,
                SpecificMissCounts = request.SpecificMissCounts,
                SpecificLargeBonusCounts = request.SpecificLargeBonusCounts,
                SpecificMaxCombos = request.SpecificMaxCombos,
                SpecificHitCounts = request.SpecificHitCounts,
                GeneralGreatCounts = request.GeneralGreatCounts,
                GeneralOkCounts = request.GeneralOkCounts,
                GeneralMissCounts = request.GeneralMissCounts,
                GeneralLargeBonusCounts = request.GeneralLargeBonusCounts,
                GeneralMaxCombos = request.GeneralMaxCombos,
                GeneralHitCounts = request.GeneralHitCounts
            });

        return Ok();
    }

    private static ExamResponse _ConstructExamResponse(ExamQuery examQuery)
    {
        return new ExamResponse
        {
            Grade = examQuery.Grade,
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
        };
    }
}

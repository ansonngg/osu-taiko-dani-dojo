﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Query;
using OsuTaikoDaniDojo.Web.Request;

namespace OsuTaikoDaniDojo.Web.Controller;

[ApiController]
[Route("api/[controller]")]
public class ExamController(IExamRepository examRepository) : ControllerBase
{
    private readonly IExamRepository _examRepository = examRepository;

    [HttpGet("{grade:int}")]
    public async Task<IActionResult> GetExamByGrade(int grade)
    {
        var examQuery = await _examRepository.GetByGradeAsync(grade);
        return Ok(new { examQuery?.BeatmapIds });
    }

    [Authorize(Roles = "Admin,Examiner")]
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
}

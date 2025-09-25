using Microsoft.AspNetCore.Mvc;
using OsuTaikoDaniDojo.Application.Interface;

namespace OsuTaikoDaniDojo.Web.Controller;

[ApiController]
[Route("api/[controller]")]
public class ExamController(IExamRepository examRepository) : ControllerBase
{
    private readonly IExamRepository _examRepository = examRepository;

    [HttpGet("{grade:int}")]
    public async Task<IActionResult> GetExamByGrade(int grade)
    {
        var examQuery = await _examRepository.GetExamByGradeAsync(grade);
        return Ok(new { examQuery?.BeatmapIds });
    }
}

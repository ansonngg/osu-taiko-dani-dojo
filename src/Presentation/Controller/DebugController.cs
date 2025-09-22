using Microsoft.AspNetCore.Mvc;

namespace OsuTaikoDaniDojo.Presentation.Controller;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok("ok");
}

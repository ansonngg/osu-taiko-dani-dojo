using Microsoft.AspNetCore.Mvc;

namespace OsuDojo.Web.Controller;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok("ok");
}

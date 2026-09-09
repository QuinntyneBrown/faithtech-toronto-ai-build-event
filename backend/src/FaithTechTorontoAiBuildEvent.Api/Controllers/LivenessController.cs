using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class LivenessController : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Get() => Ok();
}

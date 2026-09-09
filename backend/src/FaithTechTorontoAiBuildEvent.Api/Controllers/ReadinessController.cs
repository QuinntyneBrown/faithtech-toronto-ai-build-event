using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class ReadinessController(ISender sender) : ControllerBase
{
    [HttpGet("ready")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => await sender.Send(new GetReadinessQuery(), cancellationToken) ? Ok() : StatusCode(StatusCodes.Status503ServiceUnavailable);
}

using FaithTechTorontoAiBuildEvent.Application.EventState;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/event")]
public sealed class EventStateController(ISender sender) : ControllerBase
{
    [HttpGet("state")]
    public Task<PublicEventSnapshot> GetState(CancellationToken cancellationToken)
        => sender.Send(new GetPublicEventSnapshotQuery(), cancellationToken);

    [HttpGet("time")]
    public async Task<ActionResult<ServerTimeResponse>> GetTime(CancellationToken cancellationToken)
        => Ok(new ServerTimeResponse(await sender.Send(new GetServerTimeQuery(), cancellationToken)));
}

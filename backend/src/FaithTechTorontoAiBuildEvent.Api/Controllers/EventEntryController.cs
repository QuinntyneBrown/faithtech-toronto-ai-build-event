using FaithTechTorontoAiBuildEvent.Application.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/events/{eventId:guid}/entry"), AllowAnonymous]
public sealed class EventEntryController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid eventId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEntryHeaderQuery(eventId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}

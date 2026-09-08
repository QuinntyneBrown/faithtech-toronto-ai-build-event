using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/admin/events/{eventId:guid}/schedule"), Authorize(Roles = "Administrator")]
public sealed class SchedulesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid eventId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetScheduleQuery(eventId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPut, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Guid eventId, ScheduleInput input,
        [FromHeader(Name = "Idempotency-Key"), Required] Guid? operationId,
        [FromHeader(Name = "If-Match")] string? version, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new SaveScheduleCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            eventId, operationId!.Value, version, input), cancellationToken));
}

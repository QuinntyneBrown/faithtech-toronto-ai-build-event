using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FaithTechTorontoAiBuildEvent.Api.Events;
using FaithTechTorontoAiBuildEvent.Application.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/admin/events"), Authorize(Roles = "Administrator")]
public sealed class AdministratorEventsController(ISender sender) : ControllerBase
{
    [HttpPut("{eventId:guid}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Guid eventId, EventInput request,
        [FromHeader(Name = "Idempotency-Key"), Required] Guid? operationId,
        [FromHeader(Name = "If-Match")] string? version, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new SaveEventCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            eventId, operationId!.Value, version, request), cancellationToken));

    [HttpGet("{eventId:guid}")]
    public async Task<IActionResult> Get(Guid eventId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEventQuery(eventId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ListEventsQuery(), cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventRequest request,
        [FromHeader(Name = "Idempotency-Key"), Required] Guid? operationId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateEventCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            operationId!.Value, request.Title), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("{eventId:guid}/copy"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Copy(Guid eventId, CopyEventRequest request,
        [FromHeader(Name = "Idempotency-Key"), Required] Guid? operationId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CopyEventCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            eventId, operationId!.Value, request.Start), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}

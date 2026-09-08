using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FaithTechTorontoAiBuildEvent.Application.Roster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/admin/events/{eventId:guid}/roster"), Authorize(Roles = "Administrator")]
public sealed class RosterController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(Guid eventId, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ListRosterQuery(eventId), cancellationToken));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid eventId, RegistrationInput input,
        [FromHeader(Name = "Idempotency-Key"), Required] Guid? operationId, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new AddRegistrationCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            eventId, operationId!.Value, input), cancellationToken));
    [HttpPut("{registrationId:guid}/name"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(Guid eventId, Guid registrationId, RegistrationInput input,
        [FromHeader(Name = "Idempotency-Key"), Required] Guid? operationId,
        [FromHeader(Name = "If-Match")] string? version, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new RenameRegistrationCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            eventId, registrationId, operationId!.Value, version, input.DisplayName), cancellationToken));
}

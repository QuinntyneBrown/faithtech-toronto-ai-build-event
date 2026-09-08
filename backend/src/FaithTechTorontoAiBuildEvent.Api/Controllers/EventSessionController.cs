using System.Security.Claims;
using FaithTechTorontoAiBuildEvent.Api.Access;
using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/events/{eventId:guid}/session")]
public sealed class EventSessionController(ISender sender) : ControllerBase
{
    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Authenticate(Guid eventId, AuthenticateParticipantRequest request, CancellationToken cancellationToken)
    {
        var session = await sender.Send(new AuthenticateParticipantCommand(eventId, request.Email, request.EntryCode), cancellationToken);
        if (session is null) return Unauthorized();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Sid, session.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, session.RegistrationId.ToString()),
            new Claim("eventId", session.EventId.ToString())
        ], "Participant");
        await HttpContext.SignInAsync("Participant", new ClaimsPrincipal(identity), new AuthenticationProperties
        {
            IsPersistent = true, ExpiresUtc = session.AuthenticatedAtUtc.AddHours(24)
        });
        return Ok(new EntryResult(session.RegistrationId, session.EventId, session.AuthenticatedAtUtc.AddHours(24)));
    }

    [HttpGet, Authorize(AuthenticationSchemes = "Participant")]
    public async Task<IActionResult> Read(Guid eventId, CancellationToken cancellationToken)
    {
        var state = await sender.Send(new GetParticipantSessionQuery(Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!)), cancellationToken);
        return state is null ? Unauthorized() : Ok(state);
    }
}

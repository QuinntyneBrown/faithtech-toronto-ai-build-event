using System.Security.Claims;
using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/admin/session")]
public sealed class AdministratorSessionController(ISender sender) : ControllerBase
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Authenticate(AuthenticateAdministratorCommand command, CancellationToken cancellationToken)
    {
        var session = await sender.Send(command, cancellationToken);
        if (session is null) return Unauthorized();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Sid, session.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, session.AdministratorId.ToString()),
            new Claim(ClaimTypes.Role, "Administrator")
        ], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity), new AuthenticationProperties
        {
            IsPersistent = true, ExpiresUtc = session.AuthenticatedAtUtc.AddHours(8)
        });
        return NoContent();
    }

    [HttpGet, Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Read(CancellationToken cancellationToken)
    {
        var session = await sender.Send(new GetAdministratorSessionQuery(Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!)), cancellationToken);
        if (session is null) return Unauthorized();
        return Ok(new { actorId = session.AdministratorId, absoluteExpiresAtUtc = session.AuthenticatedAtUtc.AddHours(8),
            idleExpiresAtUtc = session.LastInteractionAtUtc.AddMinutes(30) });
    }

    [HttpDelete, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
    {
        await sender.Send(new SignOutAdministratorCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!)), cancellationToken);
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    [HttpPost("interaction"), Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Interact(CancellationToken cancellationToken) =>
        await sender.Send(new RecordAdministratorInteractionCommand(Guid.Parse(User.FindFirstValue(ClaimTypes.Sid)!)), cancellationToken)
            ? NoContent() : Unauthorized();
}

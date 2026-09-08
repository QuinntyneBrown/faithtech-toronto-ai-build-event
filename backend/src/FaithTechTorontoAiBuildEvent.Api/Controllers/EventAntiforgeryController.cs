using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/events/{eventId:guid}/antiforgery"), AllowAnonymous]
public sealed class EventAntiforgeryController(IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Read(Guid eventId)
    {
        // Anonymous by design (usable before sign-in), but a token generated while HttpContext.User reflects no
        // Participant identity would fail validation on a later Participant-authorized request whose antiforgery
        // token is checked against the identity present at that time. Authenticating the scheme here (when the
        // cookie is already present) keeps token generation and later validation consistent.
        var participant = await HttpContext.AuthenticateAsync("Participant");
        if (participant.Succeeded) HttpContext.User = participant.Principal!;
        return Ok(new { requestToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
    }
}

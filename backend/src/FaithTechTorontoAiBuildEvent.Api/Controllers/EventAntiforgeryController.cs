using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/events/{eventId:guid}/antiforgery"), AllowAnonymous]
public sealed class EventAntiforgeryController(IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public IActionResult Read(Guid eventId) => Ok(new { requestToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
}

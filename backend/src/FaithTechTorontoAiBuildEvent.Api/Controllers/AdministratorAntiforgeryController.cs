using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController, Route("api/admin/antiforgery")]
public sealed class AdministratorAntiforgeryController(IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public IActionResult Read() => Ok(new { requestToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
}

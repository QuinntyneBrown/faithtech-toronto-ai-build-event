using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

[ApiController]
[Route("api/admin/session")]
public sealed class AdministratorSessionController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AdministratorSessionResponse>> Authenticate(AuthenticateAdministratorRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AuthenticateAdministratorCommand(request.Passcode), cancellationToken);
        if (!result.Authenticated || result.SessionSecret is null)
        {
            return Unauthorized();
        }

        Response.Cookies.Append("faithtech-admin", result.SessionSecret, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/admin",
            Expires = DateTimeOffset.UtcNow.AddHours(8),
            IsEssential = true
        });
        return Ok(new AdministratorSessionResponse(true));
    }

    [HttpGet]
    public async Task<ActionResult<AdministratorSessionResponse>> Get(CancellationToken cancellationToken)
    {
        var active = await sender.Send(new GetAdministratorSessionQuery(Request.Cookies["faithtech-admin"]), cancellationToken);
        return active ? Ok(new AdministratorSessionResponse(true)) : Unauthorized();
    }
}

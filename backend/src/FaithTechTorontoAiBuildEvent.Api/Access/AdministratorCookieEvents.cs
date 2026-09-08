using System.Security.Claims;
using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FaithTechTorontoAiBuildEvent.Api.Access;

public sealed class AdministratorCookieEvents(ISender sender) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.Sid), out var id) ||
            await sender.Send(new GetAdministratorSessionQuery(id), context.HttpContext.RequestAborted) is null)
            context.RejectPrincipal();
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    }
}

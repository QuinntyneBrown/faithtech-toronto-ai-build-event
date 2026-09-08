using System.Security.Claims;
using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FaithTechTorontoAiBuildEvent.Api.Access;

/// <summary>Scopes the participant cookie to its authenticated event. Cookie path isolation alone is not a security
/// boundary (a non-browser client can replay the value cross-path), so ValidatePrincipal also compares the session's
/// bound event claim against the event id in the request path and rejects on any mismatch.</summary>
public sealed class ParticipantCookieEvents(ISender sender) : CookieAuthenticationEvents
{
    public override Task SigningIn(CookieSigningInContext context)
    {
        if (Guid.TryParse(context.Principal?.FindFirstValue("eventId"), out var eventId))
            context.CookieOptions.Path = $"/api/events/{eventId:D}";
        return Task.CompletedTask;
    }

    public override Task SigningOut(CookieSigningOutContext context)
    {
        if (Guid.TryParse(context.HttpContext.User.FindFirstValue("eventId"), out var eventId))
            context.CookieOptions.Path = $"/api/events/{eventId:D}";
        return Task.CompletedTask;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.Sid), out var sessionId) ||
            !Guid.TryParse(context.Principal?.FindFirstValue("eventId"), out var boundEventId) ||
            !TryGetRouteEventId(context.HttpContext, out var routeEventId) || routeEventId != boundEventId ||
            await sender.Send(new GetParticipantSessionQuery(sessionId), context.HttpContext.RequestAborted) is null)
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

    private static bool TryGetRouteEventId(HttpContext http, out Guid eventId)
    {
        eventId = Guid.Empty;
        var segments = http.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var index = segments is null ? -1 : Array.IndexOf(segments, "events");
        return index >= 0 && index + 1 < segments!.Length && Guid.TryParse(segments[index + 1], out eventId);
    }
}

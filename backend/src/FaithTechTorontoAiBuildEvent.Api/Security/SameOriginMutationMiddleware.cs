namespace FaithTechTorontoAiBuildEvent.Api.Security;

public sealed class SameOriginMutationMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (IsUnsafe(context.Request.Method) && context.Request.Headers.Origin is { Count: > 0 } origins && !IsSameOrigin(origins[0], context.Request.Host))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        return next(context);
    }

    private static bool IsUnsafe(string method)
        => !HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) && !HttpMethods.IsOptions(method);

    private static bool IsSameOrigin(string? origin, HostString host)
        => Uri.TryCreate(origin, UriKind.Absolute, out var originUri)
           && string.Equals(originUri.Authority, host.Value, StringComparison.OrdinalIgnoreCase);
}

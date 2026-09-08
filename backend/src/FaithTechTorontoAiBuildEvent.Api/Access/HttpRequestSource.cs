using FaithTechTorontoAiBuildEvent.Application.Access;

namespace FaithTechTorontoAiBuildEvent.Api.Access;

public sealed class HttpRequestSource(IHttpContextAccessor context) : IRequestSource
{
    public string Address => context.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "local";
}

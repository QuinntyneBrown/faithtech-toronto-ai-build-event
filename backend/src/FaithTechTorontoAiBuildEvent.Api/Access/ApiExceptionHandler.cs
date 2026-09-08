using FaithTechTorontoAiBuildEvent.Application.Access;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FaithTechTorontoAiBuildEvent.Api.Access;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception is AuthenticationThrottledException ? 429 : exception is SqlException ? 503 : 500;
        if (exception is AuthenticationThrottledException throttled)
            context.Response.Headers.RetryAfter = throttled.RetryAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status, Title = status == 429 ? "Try again later." : "The request could not be completed.",
            Extensions = { ["code"] = status == 429 ? "authentication-throttled" : "temporarily-unavailable",
                ["correlationId"] = context.TraceIdentifier }
        }, cancellationToken);
        return true;
    }
}

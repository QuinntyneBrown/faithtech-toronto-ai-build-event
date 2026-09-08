using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FaithTechTorontoAiBuildEvent.Api.Access;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch { AuthenticationThrottledException => 429, InputValidationException => 422,
            OperationConflictException or StaleVersionException => 409, VersionRequiredException => 428,
            ResourceNotFoundException => 404, SqlException => 503, _ => 500 };
        if (exception is AuthenticationThrottledException throttled)
            context.Response.Headers.RetryAfter = throttled.RetryAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
        context.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status, Title = status == 429 ? "Try again later." : "The request could not be completed.",
            Extensions = { ["code"] = status switch { 429 => "authentication-throttled", 422 => "validation-failed",
                409 => "operation-key-reused", _ => "temporarily-unavailable" },
                ["correlationId"] = context.TraceIdentifier }
        };
        if (exception is InputValidationException invalid) problem.Extensions["errors"] = invalid.Errors;
        if (exception is StaleVersionException stale)
        {
            problem.Extensions["code"] = "stale-version";
            problem.Extensions["current"] = stale.Current;
        }
        if (exception is VersionRequiredException) problem.Extensions["code"] = "version-required";
        if (exception is ResourceNotFoundException) problem.Extensions["code"] = "not-found";
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}

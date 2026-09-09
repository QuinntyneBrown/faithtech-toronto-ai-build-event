using System.CommandLine;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class OperatorCommandRunner
{
    public static async Task<int> Run(ParseResult parse, OperatorOptions options, CancellationToken token,
        Func<OperatorSession, CancellationToken, Task<object?>> action)
    {
        var watch = Stopwatch.StartNew(); OperatorSession? session = null; OperatorResult result; int exit;
        try {
            session = await OperatorSession.Open(parse, options, token);
            var data = await action(session, token);
            exit = session.ExitCode;
            result = new(session.OperationId, session.PreviewId, session.Principal, session.Outcome, watch.Elapsed.TotalMilliseconds, data, null);
        } catch (Exception error) when (error is ArgumentException or InvalidOperationException or IOException or JsonException or
            CryptographicException or InputValidationException or UnauthorizedAccessException or SqlException or DbUpdateException or
            StaleVersionException or OperationConflictException or ResourceNotFoundException or OperationCanceledException) {
            exit = error switch {
                StaleVersionException or OperationConflictException => 4,
                UnauthorizedAccessException => 3,
                SqlException sql when sql.Number is 18456 or 229 or 230 => 3,
                OperationCanceledException when session?.Started != true => 5,
                SqlException or DbUpdateException or OperationCanceledException when session?.Started == true => 6,
                SqlException => 3,
                _ => 2 };
            if (session?.Committed == true) exit = 7;
            var outcome = exit switch { 5 => "cancelled", 6 => "unconfirmed", 7 => "delivery-failed", _ => "rejected" };
            var message = exit switch {
                3 => "Database authentication, permissions or connection failed; no fallback was used.",
                4 => "Reviewed input or operation conflicts with current state; inspect before making a new preview.",
                5 => "Cancelled before execution.",
                6 => "Database outcome unconfirmed. Reconcile this operation before retrying.",
                7 => "Change committed; result delivery failed. Reconcile instead of repeating the mutation.",
                _ => "Invalid input, configuration or artifact. Verify help, schema compatibility and protected configuration." };
            if (error is InputValidationException invalid) message += " Fields: " + string.Join(", ", invalid.Errors.Keys);
            result = new(session?.OperationId, session?.PreviewId, session?.Principal, outcome, watch.Elapsed.TotalMilliseconds, null, message);
            if (session?.Started == true && !session.Committed && error is SqlException or DbUpdateException or OperationCanceledException or IOException) {
                var recovered = await OperatorRecovery.Resolve(session, watch.Elapsed.TotalMilliseconds, error is OperationCanceledException);
                if (recovered is not null) { result = recovered; exit = session.ExitCode; }
                else { exit = 6; result = result with { Outcome = "unconfirmed", Error = "Outcome unconfirmed; reconcile before retrying." }; }
            }
        }
        try {
            if (session?.OperationId is { } operation)
                session.Files.Journal(operation, new { operationId = operation, session.PreviewId, session.Principal, result.Outcome, result.DurationMs, atUtc = DateTimeOffset.UtcNow });
            CommittedResultOutput.Write([JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = !parse.GetValue(options.Json) })]);
        } catch (IOException) {
            Console.Error.WriteLine($"Result delivery failed; operation {session?.OperationId}; outcome {result.Outcome}.");
            exit = session?.Committed == true ? 7 : exit == 6 ? 6 : 2;
        } finally { if (session is not null) await session.DisposeAsync(); }
        return exit;
    }
}

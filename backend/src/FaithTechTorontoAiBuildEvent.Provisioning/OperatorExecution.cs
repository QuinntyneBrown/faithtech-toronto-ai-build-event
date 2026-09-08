using FaithTechTorontoAiBuildEvent.Infrastructure;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

internal static class OperatorExecution
{
    public static async Task<int> Run(Func<IServiceProvider, CancellationToken, Task> operation,
        CancellationToken cancellationToken, bool reportTarget = false)
    {
        var completed = false;
        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Logging.ClearProviders();
            builder.Services.AddEventInfrastructure(builder.Configuration);
            using var host = builder.Build();
            using var scope = host.Services.CreateScope();
            if (reportTarget) await ReportTarget(scope.ServiceProvider, cancellationToken);
            await operation(scope.ServiceProvider, cancellationToken);
            completed = true;
            Console.WriteLine("Operation completed.");
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Console.Error.WriteLine("Operation canceled. Verify database state before retrying.");
            return reportTarget ? 6 : 1;
        }
        catch (Exception exception) when (exception is SqlException or DbUpdateException)
        {
            Console.Error.WriteLine("Database operation did not return confirmed success. Inspect account state before retrying.");
            return reportTarget ? 6 : 1;
        }
        catch (IOException exception)
        {
            var committed = completed || exception is CommittedOutputException;
            Console.Error.WriteLine(committed ? "Change committed; result delivery failed. Do not repeat the mutation." : "Input/output failed before confirmed completion.");
            return committed ? 7 : 1;
        }
        catch (Exception exception) when (exception is InvalidOperationException or OptionsValidationException or ArgumentException)
        {
            Console.Error.WriteLine("Operation failed. Verify configuration, database access, account name and password policy.");
            return 1;
        }
    }

    public static void WriteCommittedResult(Action write)
    {
        try { write(); }
        catch (IOException) { throw new CommittedOutputException(); }
    }

    private static async Task ReportTarget(IServiceProvider services, CancellationToken cancellationToken)
    {
        var db = services.GetRequiredService<EventDbContext>();
        await db.Database.OpenConnectionAsync(cancellationToken);
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT @@SERVERNAME, DB_NAME(), USER_NAME()";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Database identity unavailable.");
        Console.WriteLine($"Server: {reader.GetString(0)}; Database: {reader.GetString(1)}; Principal: {reader.GetString(2)}");
    }
}

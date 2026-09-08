using FaithTechTorontoAiBuildEvent.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

internal static class OperatorExecution
{
    public static async Task<int> Run(Func<IServiceProvider, CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Logging.ClearProviders();
            builder.Services.AddEventInfrastructure(builder.Configuration);
            using var host = builder.Build();
            using var scope = host.Services.CreateScope();
            await operation(scope.ServiceProvider, cancellationToken);
            Console.WriteLine("Operation completed.");
            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Console.Error.WriteLine("Operation canceled. Verify database state before retrying.");
            return 1;
        }
        catch (Exception exception) when (exception is InvalidOperationException or SqlException or OptionsValidationException)
        {
            Console.Error.WriteLine("Operation failed. Verify configuration, database access, account name and password policy.");
            return 1;
        }
    }
}

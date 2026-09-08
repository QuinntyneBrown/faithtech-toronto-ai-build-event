using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length is < 1 or > 2 || (args[0] != "migrate" &&
            (args.Length != 2 || args[0] is not ("create-admin" or "disable-admin"))))
        {
            Console.Error.WriteLine("Usage: migrate | create-admin <username> | disable-admin <username>");
            return 2;
        }
        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Logging.ClearProviders();
            builder.Services.AddEventInfrastructure(builder.Configuration);
            using var host = builder.Build();
            using var scope = host.Services.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            if (args[0] == "migrate")
                await scope.ServiceProvider.GetRequiredService<EventDbContext>().Database.MigrateAsync();
            else if (args[0] == "create-admin")
                await sender.Send(new ProvisionAdministratorCommand(args[1], SecretInput.ReadPassword()));
            else await sender.Send(new DisableAdministratorCommand(args[1]));
            Console.WriteLine("Operation completed.");
            return 0;
        }
        catch (Exception exception) when (exception is InvalidOperationException or Microsoft.Data.SqlClient.SqlException)
        {
            Console.Error.WriteLine("Operation failed. Verify configuration, database access, account name and password policy.");
            return 1;
        }
    }
}

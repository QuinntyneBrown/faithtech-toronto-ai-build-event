using System.CommandLine;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class MigrateCommand : Command
{
    public MigrateCommand() : base("migrate", "Apply application migrations using ConnectionStrings__EventDatabase.")
    {
        SetAction((_, cancellationToken) => OperatorExecution.Run((services, token) =>
            services.GetRequiredService<EventDbContext>().Database.MigrateAsync(token), cancellationToken));
    }
}

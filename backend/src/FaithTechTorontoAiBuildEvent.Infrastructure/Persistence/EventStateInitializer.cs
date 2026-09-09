using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DomainEventState = FaithTechTorontoAiBuildEvent.Domain.EventFlow.EventState;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

public sealed class EventStateInitializer(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
        if (database.Database.IsRelational())
        {
            await database.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await database.Database.EnsureCreatedAsync(cancellationToken);
        }
        if (await database.EventStates.AnyAsync(cancellationToken))
        {
            return;
        }

        database.EventStates.Add(new DomainEventState());
        database.Projects.Add(new Project
        {
            Id = Guid.NewGuid(),
            Title = "RTR — Reconciliation Through Relationships",
            Description = "A shared-learning and relationship project supporting facilitator-reviewed matching with mutual consent."
        });
        await database.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

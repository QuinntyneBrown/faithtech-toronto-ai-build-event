using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed class EventChangePublisher(IServiceScopeFactory scopes, IHubContext<EventUpdatesHub> hubContext) : BackgroundService
{
    private long cursor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using (var scope = scopes.CreateAsyncScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
            cursor = await database.EventChanges.Select(change => (long?)change.Version).MaxAsync(stoppingToken) ?? 0;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PublishPendingAsync(stoppingToken);
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
        var version = await database.EventChanges.Where(change => change.Version > cursor)
            .Select(change => (long?)change.Version).MaxAsync(cancellationToken);
        if (version is null) return;
        await hubContext.Clients.All.SendAsync("eventUpdated", version.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), cancellationToken);
        cursor = version.Value;
    }
}

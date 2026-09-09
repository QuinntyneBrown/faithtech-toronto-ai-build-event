using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using Microsoft.AspNetCore.SignalR;

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed class PrivateSessionInvalidationWatcher(
    IServiceScopeFactory scopes,
    IPrivateConnectionRegistry connections,
    IHubContext<AdministratorUpdatesHub> administratorHub,
    IHubContext<ParticipantUpdatesHub> participantHub) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var connection in connections.Snapshot())
            {
                await using var scope = scopes.CreateAsyncScope();
                var active = connection.SessionKind == PrivateSessionKind.Administrator
                    ? await scope.ServiceProvider.GetRequiredService<IAdministratorAuthorizationStore>()
                        .IsAuthorizedAsync(connection.SecretDigest, DateTimeOffset.UtcNow, stoppingToken)
                    : await scope.ServiceProvider.GetRequiredService<IParticipantSessionStore>()
                        .FindActiveAsync(connection.SecretDigest, DateTimeOffset.UtcNow, stoppingToken) is not null;
                if (active || !connections.Remove(connection.ConnectionId)) continue;

                var clients = connection.SessionKind == PrivateSessionKind.Administrator
                    ? administratorHub.Clients
                    : participantHub.Clients;
                await clients.Client(connection.ConnectionId).SendAsync("sessionInvalidated", cancellationToken: stoppingToken);
            }
        }
    }
}

using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using Microsoft.AspNetCore.SignalR;

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed class AdministratorUpdatesHub(
    IAdministratorAuthorizationStore authorizationStore,
    IEntryReceiptSecretService secretService,
    IPrivateConnectionRegistry connections) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var secret = Context.GetHttpContext()?.Request.Cookies["faithtech-admin"];
        if (string.IsNullOrEmpty(secret) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(secret), DateTimeOffset.UtcNow, Context.ConnectionAborted))
        {
            Context.Abort();
            return;
        }
        connections.Add(new PrivateConnectionRegistration(Context.ConnectionId, PrivateSessionKind.Administrator, secretService.Digest(secret)));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        connections.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

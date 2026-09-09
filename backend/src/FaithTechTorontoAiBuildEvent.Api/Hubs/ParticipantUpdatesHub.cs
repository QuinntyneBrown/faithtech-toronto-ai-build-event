using FaithTechTorontoAiBuildEvent.Application.Participants;
using Microsoft.AspNetCore.SignalR;

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed class ParticipantUpdatesHub(
    IParticipantSessionStore sessions,
    IEntryReceiptSecretService secretService,
    IPrivateConnectionRegistry connections) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var secret = Context.GetHttpContext()?.Request.Cookies["faithtech-participant"];
        if (string.IsNullOrEmpty(secret) || await sessions.FindActiveAsync(secretService.Digest(secret), DateTimeOffset.UtcNow, Context.ConnectionAborted) is null)
        {
            Context.Abort();
            return;
        }
        connections.Add(new PrivateConnectionRegistration(Context.ConnectionId, PrivateSessionKind.Participant, secretService.Digest(secret)));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        connections.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

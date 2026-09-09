using FaithTechTorontoAiBuildEvent.Application.Participants;
using Microsoft.AspNetCore.SignalR;

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed class ParticipantUpdatesHub(IParticipantSessionStore sessions, IEntryReceiptSecretService secretService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var secret = Context.GetHttpContext()?.Request.Cookies["faithtech-participant"];
        if (string.IsNullOrEmpty(secret) || await sessions.FindActiveAsync(secretService.Digest(secret), DateTimeOffset.UtcNow, Context.ConnectionAborted) is null)
        {
            Context.Abort();
            return;
        }
        await base.OnConnectedAsync();
    }
}

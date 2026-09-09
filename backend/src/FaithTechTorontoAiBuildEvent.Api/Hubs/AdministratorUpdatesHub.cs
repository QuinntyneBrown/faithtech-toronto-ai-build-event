using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using Microsoft.AspNetCore.SignalR;

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed class AdministratorUpdatesHub(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var secret = Context.GetHttpContext()?.Request.Cookies["faithtech-admin"];
        if (string.IsNullOrEmpty(secret) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(secret), DateTimeOffset.UtcNow, Context.ConnectionAborted))
        {
            Context.Abort();
            return;
        }
        await base.OnConnectedAsync();
    }
}

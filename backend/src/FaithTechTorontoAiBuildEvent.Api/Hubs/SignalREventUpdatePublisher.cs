using FaithTechTorontoAiBuildEvent.Application.EventState;
using Microsoft.AspNetCore.SignalR;

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed class SignalREventUpdatePublisher(IHubContext<EventUpdatesHub> hubContext) : IEventUpdatePublisher
{
    public Task PublishAsync(long version, CancellationToken cancellationToken)
        => hubContext.Clients.All.SendAsync("eventUpdated", version.ToString(System.Globalization.CultureInfo.InvariantCulture), cancellationToken);
}

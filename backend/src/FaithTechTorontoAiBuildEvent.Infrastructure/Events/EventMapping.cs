using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Domain.Events;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Events;

public static class EventMapping
{
    public static EventInput Input(BuildEvent item) => new(item.Title, item.VenueName, item.Address, item.Latitude, item.Longitude,
        item.WaitingContent, item.ClosingContent, item.DirectionsUrl, item.Timezone,
        item.StartLocal is { } start ? new(start, item.StartOffsetMinutes) : null,
        item.EndLocal is { } end ? new(end, item.EndOffsetMinutes) : null, item.UseLiturgy);

    public static void Apply(BuildEvent item, EventInput input)
    {
        item.Title = input.Title; item.VenueName = input.VenueName; item.Address = input.Address;
        item.UseLiturgy = input.UseLiturgy; item.Latitude = input.Latitude; item.Longitude = input.Longitude;
        item.WaitingContent = input.WaitingContent; item.ClosingContent = input.ClosingContent; item.DirectionsUrl = input.DirectionsUrl;
        item.Timezone = input.Timezone; item.StartLocal = input.Start?.Local; item.EndLocal = input.End?.Local;
        item.StartOffsetMinutes = input.Start?.OffsetMinutes; item.EndOffsetMinutes = input.End?.OffsetMinutes;
        item.StartsAtUtc = input.Start?.ToUtc(); item.EndsAtUtc = input.End?.ToUtc();
    }
}

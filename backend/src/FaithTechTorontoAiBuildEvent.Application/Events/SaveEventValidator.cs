using FaithTechTorontoAiBuildEvent.Application.Validation;
using static FaithTechTorontoAiBuildEvent.Application.Validation.TextValidation;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public static class SaveEventValidator
{
    public static EventInput Normalize(EventInput input)
    {
        var result = input with { Title = Text(input.Title, "title", 200), VenueName = Text(input.VenueName, "venueName", 200),
            Address = Text(input.Address, "address", 5000), WaitingContent = Text(input.WaitingContent, "waitingContent", 5000),
            ClosingContent = Text(input.ClosingContent, "closingContent", 5000), DirectionsUrl = HttpsUrl(input.DirectionsUrl, "directionsUrl") };
        if (input.Latitude is { } latitude && (!double.IsFinite(latitude) || latitude is < -90 or > 90))
            throw new InputValidationException("latitude", "Use a latitude between -90 and 90.");
        if (input.Longitude is { } longitude && (!double.IsFinite(longitude) || longitude is < -180 or > 180))
            throw new InputValidationException("longitude", "Use a longitude between -180 and 180.");
        var timezone = Text(input.Timezone, "timezone", 200);
        var zone = LocalTimeResolver.Zone(timezone);
        var start = LocalTimeResolver.Resolve(input.Start, zone, "start");
        var end = LocalTimeResolver.Resolve(input.End, zone, "end");
        if (start?.ToUtc() is { } startsAt && end?.ToUtc() is { } endsAt && endsAt <= startsAt)
            throw new InputValidationException("end", "Event end must be after event start.");
        if (start is not null && end is not null && (start.OffsetMinutes is null || end.OffsetMinutes is null) && end.Local <= start.Local)
            throw new InputValidationException("end", "Event end must be after event start.");
        return result with { Timezone = timezone, Start = start, End = end };
    }

}

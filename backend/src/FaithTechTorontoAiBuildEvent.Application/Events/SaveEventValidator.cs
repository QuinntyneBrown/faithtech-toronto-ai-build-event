using FaithTechTorontoAiBuildEvent.Application.Validation;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public static class SaveEventValidator
{
    public static EventInput Normalize(EventInput input)
    {
        var result = input with { Title = Text(input.Title, "title", 200), VenueName = Text(input.VenueName, "venueName", 200),
            Address = Text(input.Address, "address", 5000), WaitingContent = Text(input.WaitingContent, "waitingContent", 5000),
            ClosingContent = Text(input.ClosingContent, "closingContent", 5000), DirectionsUrl = Text(input.DirectionsUrl, "directionsUrl", 2048) };
        if (result.DirectionsUrl is not null && (!Uri.TryCreate(result.DirectionsUrl, UriKind.Absolute, out var url) ||
            url.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(url.Host) || url.UserInfo.Length > 0))
            throw new InputValidationException("directionsUrl", "Use an absolute HTTPS URL without credentials.");
        if (input.Latitude is { } latitude && (!double.IsFinite(latitude) || latitude is < -90 or > 90))
            throw new InputValidationException("latitude", "Use a latitude between -90 and 90.");
        if (input.Longitude is { } longitude && (!double.IsFinite(longitude) || longitude is < -180 or > 180))
            throw new InputValidationException("longitude", "Use a longitude between -180 and 180.");
        return result;
    }

    private static string? Text(string? value, string field, int limit)
    {
        var normalized = value?.ReplaceLineEndings("\n").Trim();
        if (normalized?.EnumerateRunes().Count() > limit) throw new InputValidationException(field, $"Use at most {limit} characters.");
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}

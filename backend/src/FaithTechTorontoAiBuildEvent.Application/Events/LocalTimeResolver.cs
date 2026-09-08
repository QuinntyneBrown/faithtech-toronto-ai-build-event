using FaithTechTorontoAiBuildEvent.Application.Validation;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public static class LocalTimeResolver
{
    public static TimeZoneInfo? Zone(string? id)
    {
        if (id is null) return null;
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch (Exception error) when (error is TimeZoneNotFoundException or InvalidTimeZoneException)
        { throw new InputValidationException("timezone", "Select a recognized timezone."); }
    }

    public static LocalTimeInput? Resolve(LocalTimeInput? input, TimeZoneInfo? zone, string field)
    {
        if (input is null) return null;
        if (input.Local.Kind != DateTimeKind.Unspecified)
            throw new InputValidationException(field, "Supply a local date and time, with its offset in the separate offset field.");
        if (input.OffsetMinutes is < -840 or > 840) throw new InputValidationException(field, "Use a valid UTC offset in minutes.");
        if (zone is not null)
        {
            if (zone.IsInvalidTime(input.Local)) throw new InputValidationException(field, "This local time does not exist in the timezone. Choose another time.");
            var offsets = zone.IsAmbiguousTime(input.Local) ? zone.GetAmbiguousTimeOffsets(input.Local) : [zone.GetUtcOffset(input.Local)];
            if (input.OffsetMinutes is null && offsets.Length > 1)
                throw new InputValidationException(field, $"This local time occurs twice. Choose UTC offset minutes: {string.Join(", ", offsets.Select(x => x.TotalMinutes))}.");
            if (input.OffsetMinutes is { } offset && !offsets.Contains(TimeSpan.FromMinutes(offset)))
                throw new InputValidationException(field, "The offset does not match this local time and timezone.");
            input = input with { OffsetMinutes = input.OffsetMinutes ?? (int)offsets[0].TotalMinutes };
        }
        try { _ = input.ToUtc(); }
        catch (ArgumentException) { throw new InputValidationException(field, "The resolved time is outside the supported date range."); }
        return input;
    }
}

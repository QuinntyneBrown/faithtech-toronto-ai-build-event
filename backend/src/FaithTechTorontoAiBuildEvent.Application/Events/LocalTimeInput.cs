namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record LocalTimeInput(DateTime Local, int? OffsetMinutes)
{
    public DateTimeOffset? ToUtc() => OffsetMinutes is { } offset ? new DateTimeOffset(Local, TimeSpan.FromMinutes(offset)).ToUniversalTime() : null;
}

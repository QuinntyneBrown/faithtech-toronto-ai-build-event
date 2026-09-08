namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed record RosterEntry(Guid Id, string DisplayName, bool Active, bool EmailBound, DateTimeOffset? FirstAccessAtUtc, string Version);

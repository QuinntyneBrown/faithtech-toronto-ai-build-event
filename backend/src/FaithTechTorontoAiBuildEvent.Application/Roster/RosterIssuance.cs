namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed record RosterIssuance(RosterEntry Entry, string? Code, bool PreviouslyCompleted, Guid CredentialVersion);

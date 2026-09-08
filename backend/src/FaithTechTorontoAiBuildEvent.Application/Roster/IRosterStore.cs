namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public interface IRosterStore
{
    Task<IReadOnlyList<RosterEntry>> List(Guid eventId, CancellationToken cancellationToken);
    Task<RosterIssuance> Add(AddRegistrationCommand command, CancellationToken cancellationToken);
    Task<RosterEntry> Rename(RenameRegistrationCommand command, CancellationToken cancellationToken);
    Task<RosterEntry> Deactivate(DeactivateRegistrationCommand command, CancellationToken cancellationToken);
    Task<RosterIssuance> ReplaceCode(ReplaceEntryCodeCommand command, CancellationToken cancellationToken);
    Task<RosterEntry> Reactivate(ReactivateRegistrationCommand command, CancellationToken cancellationToken);
}

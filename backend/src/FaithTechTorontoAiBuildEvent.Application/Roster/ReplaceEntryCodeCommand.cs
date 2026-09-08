using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed record ReplaceEntryCodeCommand(Guid ActorId, Guid EventId, Guid RegistrationId, Guid OperationId, string? Version, bool ClearEmailBinding) : IRequest<RosterIssuance>;

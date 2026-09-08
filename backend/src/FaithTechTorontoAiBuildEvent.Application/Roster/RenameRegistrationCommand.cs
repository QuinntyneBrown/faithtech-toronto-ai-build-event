using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed record RenameRegistrationCommand(Guid ActorId, Guid EventId, Guid RegistrationId, Guid OperationId, string? Version, string? DisplayName) : IRequest<RosterEntry>;

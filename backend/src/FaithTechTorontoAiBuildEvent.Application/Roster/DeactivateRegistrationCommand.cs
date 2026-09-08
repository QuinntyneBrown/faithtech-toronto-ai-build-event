using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed record DeactivateRegistrationCommand(Guid ActorId, Guid EventId, Guid RegistrationId, Guid OperationId, string? Version) : IRequest<RosterEntry>;

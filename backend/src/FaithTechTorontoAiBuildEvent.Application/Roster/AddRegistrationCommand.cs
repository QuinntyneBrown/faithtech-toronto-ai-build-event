using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed record AddRegistrationCommand(Guid ActorId, Guid EventId, Guid OperationId, RegistrationInput Input) : IRequest<RosterIssuance>;

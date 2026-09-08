using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record SaveEventCommand(Guid ActorId, Guid EventId, Guid OperationId, string? Version, EventInput Input) : IRequest<EventDetail>;

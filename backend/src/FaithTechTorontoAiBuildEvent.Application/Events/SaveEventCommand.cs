using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record SaveEventCommand(Guid ActorId, Guid EventId, Guid OperationId, string? Version, string? Title) : IRequest<EventSummary>;

using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record CreateEventCommand(Guid ActorId, Guid OperationId, string? Title) : IRequest<EventSummary>;

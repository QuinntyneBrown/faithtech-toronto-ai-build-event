using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record CopyEventCommand(Guid ActorId, Guid SourceEventId, Guid OperationId, LocalTimeInput? NewStart) : IRequest<EventDetail>;

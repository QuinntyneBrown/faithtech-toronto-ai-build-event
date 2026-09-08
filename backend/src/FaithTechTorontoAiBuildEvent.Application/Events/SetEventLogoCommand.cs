using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record SetEventLogoCommand(Guid ActorId, Guid EventId, Guid OperationId, string? Version,
    Stream Content, long Length, string MediaType, string FileName) : IRequest<EventDetail>;

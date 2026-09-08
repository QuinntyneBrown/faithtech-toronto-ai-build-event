using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record GetEventQuery(Guid EventId) : IRequest<EventSummary?>;

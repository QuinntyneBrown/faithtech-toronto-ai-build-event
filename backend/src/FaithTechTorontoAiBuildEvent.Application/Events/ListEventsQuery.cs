using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record ListEventsQuery : IRequest<IReadOnlyList<EventSummary>>;

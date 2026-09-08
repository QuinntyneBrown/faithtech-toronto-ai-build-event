using FaithTechTorontoAiBuildEvent.Application.Validation;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class CreateEventHandler(IEventStore store) : IRequestHandler<CreateEventCommand, EventSummary>
{
    public Task<EventSummary> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var title = request.Title?.ReplaceLineEndings("\n").Trim();
        if (title?.EnumerateRunes().Count() > 200) throw new InputValidationException("title", "Use at most 200 characters.");
        return store.CreateDraft(request.ActorId, request.OperationId, string.IsNullOrEmpty(title) ? null : title, cancellationToken);
    }
}

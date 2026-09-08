using FaithTechTorontoAiBuildEvent.Application.Validation;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class SaveEventHandler(IEventStore store) : IRequestHandler<SaveEventCommand, EventSummary>
{
    public Task<EventSummary> Handle(SaveEventCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        var title = request.Title?.ReplaceLineEndings("\n").Trim();
        if (title?.EnumerateRunes().Count() > 200) throw new InputValidationException("title", "Use at most 200 characters.");
        return store.SaveDraft(request with { Title = string.IsNullOrEmpty(title) ? null : title, Version = request.Version.Trim('"') }, cancellationToken);
    }
}

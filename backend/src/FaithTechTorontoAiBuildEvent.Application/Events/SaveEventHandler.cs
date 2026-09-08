using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class SaveEventHandler(IEventStore store) : IRequestHandler<SaveEventCommand, EventDetail>
{
    public Task<EventDetail> Handle(SaveEventCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        return store.SaveDraft(request with { Input = SaveEventValidator.Normalize(request.Input), Version = request.Version.Trim('"') }, cancellationToken);
    }
}

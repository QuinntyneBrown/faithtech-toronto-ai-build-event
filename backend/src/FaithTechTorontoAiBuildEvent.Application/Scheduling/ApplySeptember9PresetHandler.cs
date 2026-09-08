using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed class ApplySeptember9PresetHandler(IScheduleStore store) : IRequestHandler<ApplySeptember9PresetCommand, ScheduleDetail>
{
    public Task<ScheduleDetail> Handle(ApplySeptember9PresetCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        return store.ApplyReference(new(request.ActorId, request.EventId, request.OperationId, request.Version.Trim('"'),
            ScheduleValidator.Normalize(September9Reference.Create())), cancellationToken);
    }
}

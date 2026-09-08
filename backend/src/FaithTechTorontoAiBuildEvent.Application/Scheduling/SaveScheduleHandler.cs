using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed class SaveScheduleHandler(IScheduleStore store) : IRequestHandler<SaveScheduleCommand, ScheduleDetail>
{
    public Task<ScheduleDetail> Handle(SaveScheduleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        return store.Save(request with { Version = request.Version.Trim('"'), Input = ScheduleValidator.Normalize(request.Input) }, cancellationToken);
    }
}

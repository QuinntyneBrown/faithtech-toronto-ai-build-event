using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed class GetScheduleHandler(IScheduleStore store) : IRequestHandler<GetScheduleQuery, ScheduleDetail?>
{
    public Task<ScheduleDetail?> Handle(GetScheduleQuery request, CancellationToken cancellationToken) => store.Get(request.EventId, cancellationToken);
}

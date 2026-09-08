namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public interface IScheduleStore
{
    Task<ScheduleDetail?> Get(Guid eventId, CancellationToken cancellationToken);
    Task<ScheduleDetail> Save(SaveScheduleCommand command, CancellationToken cancellationToken);
}

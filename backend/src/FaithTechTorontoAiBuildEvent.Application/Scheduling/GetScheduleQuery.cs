using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed record GetScheduleQuery(Guid EventId) : IRequest<ScheduleDetail?>;

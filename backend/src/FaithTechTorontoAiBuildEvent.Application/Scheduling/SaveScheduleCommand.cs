using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed record SaveScheduleCommand(Guid ActorId, Guid EventId, Guid OperationId, string? Version,
    ScheduleInput Input) : IRequest<ScheduleDetail>;

using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed record ApplySeptember9PresetCommand(Guid ActorId, Guid EventId, Guid OperationId, string? Version) : IRequest<ScheduleDetail>;

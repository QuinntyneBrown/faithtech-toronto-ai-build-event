using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;

namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public sealed record EventSeedState(EventInput Event, ScheduleInput Schedule);

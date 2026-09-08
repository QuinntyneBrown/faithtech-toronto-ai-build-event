using FaithTechTorontoAiBuildEvent.Application.Events;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed record ScheduleDetail(Guid Id, string Version, string? Timezone, LocalTimeInput? Start, LocalTimeInput? End,
    IReadOnlyList<StageInput> Stages, WindowInput? Selection, WindowInput? Presentation, bool SelectionClosed, bool Completed);

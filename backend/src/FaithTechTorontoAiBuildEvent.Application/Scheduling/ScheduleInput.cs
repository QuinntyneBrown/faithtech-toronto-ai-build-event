using FaithTechTorontoAiBuildEvent.Application.Events;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed record ScheduleInput(string? Timezone, LocalTimeInput? Start, LocalTimeInput? End,
    IReadOnlyList<StageInput>? Stages, WindowInput? Selection, WindowInput? Presentation);

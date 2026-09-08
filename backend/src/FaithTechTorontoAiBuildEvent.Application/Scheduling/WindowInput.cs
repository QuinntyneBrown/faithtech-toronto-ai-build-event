using FaithTechTorontoAiBuildEvent.Application.Events;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed record WindowInput(LocalTimeInput? Start, LocalTimeInput? End);

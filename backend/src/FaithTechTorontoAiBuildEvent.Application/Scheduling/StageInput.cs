using FaithTechTorontoAiBuildEvent.Application.Events;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public sealed record StageInput(Guid Id, string? Name, string? Phase, string? ScreenType, string? Content,
    string? ResourceUrl, LocalTimeInput? Start, LocalTimeInput? End);

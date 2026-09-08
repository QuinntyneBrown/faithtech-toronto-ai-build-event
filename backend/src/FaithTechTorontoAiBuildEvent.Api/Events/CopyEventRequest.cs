using FaithTechTorontoAiBuildEvent.Application.Events;

namespace FaithTechTorontoAiBuildEvent.Api.Events;

public sealed record CopyEventRequest(LocalTimeInput? Start);

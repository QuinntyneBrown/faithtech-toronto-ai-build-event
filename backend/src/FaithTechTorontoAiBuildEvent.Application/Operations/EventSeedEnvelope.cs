using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;

namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public sealed record EventSeedEnvelope(JsonElement? Event, ScheduleInput? Schedule);

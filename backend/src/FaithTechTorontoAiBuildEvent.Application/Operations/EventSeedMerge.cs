using System.Text.Json;
using System.Text.Json.Nodes;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Application.Validation;

namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public static class EventSeedMerge
{
    public static EventSeedState Merge(EventSeedEnvelope seed, EventInput current, ScheduleInput schedule, bool create)
    {
        if (create && (seed.Event is null || seed.Schedule is null))
            throw new InputValidationException("seed", "Creation requires event and schedule objects.");
        var values = JsonSerializer.SerializeToNode(current, EventSeedReader.Json)!.AsObject();
        if (seed.Event is { } patch)
            foreach (var property in patch.EnumerateObject()) values[property.Name] = JsonNode.Parse(property.Value.GetRawText());
        var settings = SaveEventValidator.Normalize(values.Deserialize<EventInput>(EventSeedReader.Json)!);
        if (seed.Schedule is { } replacement) {
            var timing = new Dictionary<string, bool> {
                ["timezone"] = settings.Timezone == replacement.Timezone,
                ["start"] = settings.Start == replacement.Start,
                ["end"] = settings.End == replacement.End };
            foreach (var pair in timing)
                if (seed.Event?.TryGetProperty(pair.Key, out _) == true && !pair.Value)
                    throw new InputValidationException(pair.Key, "Event and schedule timing must agree.");
            schedule = replacement;
            settings = settings with { Timezone = schedule.Timezone, Start = schedule.Start, End = schedule.End };
        } else schedule = ScheduleValidator.Normalize(schedule with { Timezone = settings.Timezone, Start = settings.Start, End = settings.End });
        return new(settings, schedule);
    }
}

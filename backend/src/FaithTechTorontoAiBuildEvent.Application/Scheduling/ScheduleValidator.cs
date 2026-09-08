using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using static FaithTechTorontoAiBuildEvent.Application.Validation.TextValidation;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public static class ScheduleValidator
{
    public static ScheduleInput Normalize(ScheduleInput input)
    {
        var timing = SaveEventValidator.Normalize(new(null, null, null, null, null, null, null, null, input.Timezone, input.Start, input.End));
        var zone = LocalTimeResolver.Zone(timing.Timezone);
        if (input.Stages is null) throw new InputValidationException("stages", "Supply the stage list; an empty list is valid.");
        if ((input.Stages.Count > 0 || input.Selection is not null || input.Presentation is not null) &&
            (zone is null || timing.Start?.ToUtc() is null || timing.End?.ToUtc() is null))
            throw new InputValidationException("start", "Set the event timezone, start and end before configuring stages or activity windows.");
        var stages = new List<StageInput>(); var identities = new HashSet<Guid>();
        for (var index = 0; index < input.Stages.Count; index++) {
            var stage = input.Stages[index]; var field = $"stages[{index}]";
            if (stage is null) throw new InputValidationException(field, "Supply a stage.");
            if (stage.Id == Guid.Empty || !identities.Add(stage.Id)) throw new InputValidationException(field + ".id", "Each stage needs a distinct identity.");
            var name = Text(stage.Name, field + ".name", 200) ?? throw new InputValidationException(field + ".name", "Enter a stage name.");
            var screen = Text(stage.ScreenType, field + ".screenType", 50);
            if (screen is not ("information" or "welcome" or "teams" or "projects" or "build" or "people" or "raffle" or "demos" or "recap"))
                throw new InputValidationException(field + ".screenType", "Choose a configured screen type.");
            var interval = Window(new(stage.Start, stage.End), zone, field, timing)!;
            stages.Add(stage with { Name = name, Phase = Text(stage.Phase, field + ".phase", 200), ScreenType = screen,
                Content = Text(stage.Content, field + ".content", 5000), ResourceUrl = HttpsUrl(stage.ResourceUrl, field + ".resourceUrl"),
                Start = interval.Start, End = interval.End });
        }
        var ordered = stages.Select((stage, index) => (stage, index)).OrderBy(x => x.stage.Start!.ToUtc()).ToArray();
        for (var index = 1; index < ordered.Length; index++)
            if (ordered[index].stage.Start!.ToUtc() < ordered[index - 1].stage.End!.ToUtc())
                throw new InputValidationException($"stages[{ordered[index].index}].start", $"Stage '{ordered[index].stage.Name}' overlaps '{ordered[index - 1].stage.Name}'.");
        return input with { Timezone = timing.Timezone, Start = timing.Start, End = timing.End, Stages = ordered.Select(x => x.stage).ToArray(),
            Selection = Window(input.Selection, zone, "selection", timing), Presentation = Window(input.Presentation, zone, "presentation", timing) };
    }
    private static WindowInput? Window(WindowInput? window, TimeZoneInfo? zone, string field, EventInput timing)
    {
        if (window is null) return null;
        var start = LocalTimeResolver.Resolve(window.Start, zone, field + ".start") ?? throw new InputValidationException(field + ".start", "Enter the start date and time.");
        var end = LocalTimeResolver.Resolve(window.End, zone, field + ".end") ?? throw new InputValidationException(field + ".end", "Enter the end date and time.");
        if (end.ToUtc() <= start.ToUtc()) throw new InputValidationException(field + ".end", "End must be after start.");
        if (start.ToUtc() < timing.Start!.ToUtc()) throw new InputValidationException(field + ".start", "The interval starts before the event.");
        if (end.ToUtc() > timing.End!.ToUtc()) throw new InputValidationException(field + ".end", "The interval ends after the event.");
        return new(start, end);
    }
}

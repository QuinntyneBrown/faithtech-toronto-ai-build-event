using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Domain.Events;
using FaithTechTorontoAiBuildEvent.Domain.Scheduling;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Scheduling;

public static class ScheduleMapping
{
    public static ScheduleInput Input(BuildEvent item) => new(item.Timezone,
        item.StartLocal is { } start ? new(start, item.StartOffsetMinutes) : null,
        item.EndLocal is { } end ? new(end, item.EndOffsetMinutes) : null,
        item.Stages.OrderBy(x => x.Interval.StartsAtUtc).Select(x => new StageInput(x.Id, x.Name, x.Phase, x.ScreenType,
            x.Content, x.ResourceUrl, new(x.Interval.StartLocal, x.Interval.StartOffsetMinutes), new(x.Interval.EndLocal, x.Interval.EndOffsetMinutes))).ToArray(),
        Window(item.SelectionWindow), Window(item.PresentationWindow));
    public static ScheduleDetail Detail(BuildEvent item, string version, DateTimeOffset now)
    {
        var input = Input(item);
        return new(item.Id, version, input.Timezone, input.Start, input.End, input.Stages!, input.Selection, input.Presentation,
            item.SelectionClosedAtUtc is not null || (item.Published && item.SelectionWindow?.EndsAtUtc <= now),
            item.CompletedAtUtc is not null || (item.Published && item.EndsAtUtc <= now));
    }
    public static void Apply(BuildEvent item, ScheduleInput input)
    {
        item.Timezone = input.Timezone; item.StartLocal = input.Start?.Local; item.EndLocal = input.End?.Local;
        item.StartOffsetMinutes = input.Start?.OffsetMinutes; item.EndOffsetMinutes = input.End?.OffsetMinutes;
        item.StartsAtUtc = input.Start?.ToUtc(); item.EndsAtUtc = input.End?.ToUtc();
        item.SelectionWindow = Window(input.Selection); item.PresentationWindow = Window(input.Presentation);
        var stages = input.Stages ?? throw new ArgumentException("A normalized schedule needs a stage list.", nameof(input));
        var retained = stages.Select(x => x.Id).ToHashSet(); item.Stages.RemoveAll(x => !retained.Contains(x.Id));
        foreach (var stage in stages) {
            var entity = item.Stages.SingleOrDefault(x => x.Id == stage.Id);
            if (entity is null) { entity = new() { Id = stage.Id, EventId = item.Id }; item.Stages.Add(entity); }
            entity.Name = stage.Name!; entity.Phase = stage.Phase; entity.ScreenType = stage.ScreenType!;
            entity.Content = stage.Content; entity.ResourceUrl = stage.ResourceUrl; entity.Interval = Window(new WindowInput(stage.Start, stage.End))!;
        }
    }
    private static WindowInput? Window(TimeWindow? window) => window is null ? null : new(
        new(window.StartLocal, window.StartOffsetMinutes), new(window.EndLocal, window.EndOffsetMinutes));
    private static TimeWindow? Window(WindowInput? input) => input is null ? null : new() {
        StartLocal = input.Start!.Local, EndLocal = input.End!.Local, StartOffsetMinutes = input.Start.OffsetMinutes!.Value,
        EndOffsetMinutes = input.End.OffsetMinutes!.Value, StartsAtUtc = input.Start.ToUtc()!.Value, EndsAtUtc = input.End.ToUtc()!.Value };
}

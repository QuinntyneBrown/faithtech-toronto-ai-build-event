using FaithTechTorontoAiBuildEvent.Application.Validation;
using FaithTechTorontoAiBuildEvent.Domain.Events;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public static class ScheduleClosurePolicy
{
    public static void CheckAndRecord(BuildEvent item, ScheduleInput proposed, DateTimeOffset now)
    {
        if (!item.Published) return;
        if (item.EndsAtUtc <= now) item.CompletedAtUtc ??= item.EndsAtUtc;
        if (item.SelectionWindow is { } previous) {
            if (previous.StartsAtUtc <= now) item.SelectionOpenedAtUtc ??= previous.StartsAtUtc;
            if (previous.EndsAtUtc <= now) item.SelectionClosedAtUtc ??= previous.EndsAtUtc;
        }
        if (item.CompletedAtUtc is not null && (proposed.End?.ToUtc() is not { } end || end > now))
            throw new InputValidationException("end", "A completed event cannot be reopened. Content corrections remain available.");
        if (proposed.Selection is { } selection) {
            if (item.SelectionClosedAtUtc is { } closed && selection.End!.ToUtc() > closed)
                throw new InputValidationException("selection.end", "Selection is permanently closed and cannot be reopened.");
            if (item.SelectionOpenedAtUtc is { } opened && selection.Start!.ToUtc() != opened)
                throw new InputValidationException("selection.start", "The opening time of a selection window that has opened cannot be changed.");
        } else if (item.SelectionOpenedAtUtc is not null) item.SelectionClosedAtUtc ??= now;
    }
}

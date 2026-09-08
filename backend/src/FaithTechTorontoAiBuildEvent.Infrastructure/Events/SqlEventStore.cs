using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using FaithTechTorontoAiBuildEvent.Infrastructure.Scheduling;
using FaithTechTorontoAiBuildEvent.Domain.Events;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Events;

public sealed class SqlEventStore(EventDbContext db) : IEventStore
{
    public async Task<EventDetail> SaveDraft(SaveEventCommand command, CancellationToken cancellationToken)
    {
        var target = $"PUT /api/admin/events/{command.EventId}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { target, command.Input, command.Version }))));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"event:{command.EventId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        var item = await db.Events.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == command.EventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == command.ActorId && x.EventId == command.EventId && x.OperationId == command.OperationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<EventDetail>(receipt.Result)!;
        }
        var current = await Detail(item, cancellationToken);
        if (current.Version != command.Version) throw new StaleVersionException(current);
        var input = command.Input;
        var schedule = ScheduleValidator.Normalize(ScheduleMapping.Input(item) with { Timezone = input.Timezone, Start = input.Start, End = input.End });
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        ScheduleClosurePolicy.CheckAndRecord(item, schedule, now);
        item.Title = input.Title; item.VenueName = input.VenueName; item.Address = input.Address;
        item.UseLiturgy = input.UseLiturgy;
        item.Latitude = input.Latitude; item.Longitude = input.Longitude;
        item.WaitingContent = input.WaitingContent; item.ClosingContent = input.ClosingContent; item.DirectionsUrl = input.DirectionsUrl;
        item.Timezone = input.Timezone; item.StartLocal = input.Start?.Local; item.EndLocal = input.End?.Local;
        item.StartOffsetMinutes = input.Start?.OffsetMinutes; item.EndOffsetMinutes = input.End?.OffsetMinutes;
        item.StartsAtUtc = input.Start?.ToUtc(); item.EndsAtUtc = input.End?.ToUtc();
        await db.SaveChangesAsync(cancellationToken);
        var result = await Detail(item, cancellationToken);
        db.OperationReceipts.Add(new() { ActorId = command.ActorId, EventId = item.Id, OperationId = command.OperationId,
            Target = target, PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = command.ActorId, EventId = item.Id, Action = "event-saved", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private async Task<EventDetail> Detail(BuildEvent item, CancellationToken cancellationToken) => new(item.Id, item.Title, item.Published, item.UseLiturgy,
        Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!), item.VenueName, item.Address,
        item.Latitude, item.Longitude, item.WaitingContent, item.ClosingContent, item.DirectionsUrl, item.Timezone,
        item.StartLocal is { } start ? new(start, item.StartOffsetMinutes) : null,
        item.EndLocal is { } end ? new(end, item.EndOffsetMinutes) : null, item.StartsAtUtc, item.EndsAtUtc,
        item.LogoId is null ? null : await db.Logos.Where(x => x.Id == item.LogoId)
            .Select(x => new LogoMetadata(x.Id, x.MediaType, x.Width, x.Height)).SingleAsync(cancellationToken));

    public async Task<EventDetail?> GetEvent(Guid eventId, CancellationToken cancellationToken)
    {
        var item = await db.Events.SingleOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        return item is null ? null : await Detail(item, cancellationToken);
    }

    public async Task<EventSummary> CreateDraft(Guid actorId, Guid operationId, string? title, CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { route = "POST /api/admin/events", title }))));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"operation:{actorId}:platform:{operationId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == actorId && x.EventId == null && x.OperationId == operationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<EventSummary>(receipt.Result)!;
        }
        var item = new BuildEvent { Title = title };
        db.Events.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        var version = Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!);
        var result = new EventSummary(item.Id, item.Title, item.Published, item.UseLiturgy, version);
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        db.OperationReceipts.Add(new() { ActorId = actorId, OperationId = operationId, Target = "POST /api/admin/events",
            PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = actorId, EventId = item.Id, Action = "event-created", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<EventDetail> CopyEvent(Guid actorId, Guid sourceEventId, Guid operationId, LocalTimeInput newStart, CancellationToken cancellationToken)
    {
        var target = $"POST /api/admin/events/{sourceEventId}/copy";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { target, newStart }))));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"operation:{actorId}:platform:{operationId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == actorId && x.EventId == null && x.OperationId == operationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<EventDetail>(receipt.Result)!;
        }
        var item = await db.Events.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == sourceEventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var hasDatedContent = item.EndsAtUtc is not null || item.Stages.Count > 0 || item.SelectionWindow is not null || item.PresentationWindow is not null;
        if (hasDatedContent && item.StartsAtUtc is null)
            throw new InputValidationException("start", "The source event has no resolved start; its configuration cannot be copied.");
        var zone = LocalTimeResolver.Zone(item.Timezone);
        var shift = item.StartsAtUtc is { } sourceStart ? newStart.ToUtc()!.Value - sourceStart : TimeSpan.Zero;
        LocalTimeInput? Shift(LocalTimeInput? value) => value?.ToUtc() is { } utc ? new(TimeZoneInfo.ConvertTimeFromUtc((utc + shift).UtcDateTime, zone!), null) : null;
        var sourceSchedule = ScheduleMapping.Input(item);
        var shiftedSchedule = sourceSchedule with
        {
            Start = newStart,
            End = Shift(sourceSchedule.End),
            Stages = sourceSchedule.Stages!.Select(stage => stage with { Id = Guid.NewGuid(), Start = Shift(stage.Start), End = Shift(stage.End) }).ToArray(),
            Selection = sourceSchedule.Selection is null ? null : sourceSchedule.Selection with { Start = Shift(sourceSchedule.Selection.Start), End = Shift(sourceSchedule.Selection.End) },
            Presentation = sourceSchedule.Presentation is null ? null : sourceSchedule.Presentation with { Start = Shift(sourceSchedule.Presentation.Start), End = Shift(sourceSchedule.Presentation.End) },
        };
        var normalized = ScheduleValidator.Normalize(shiftedSchedule);
        var copy = new BuildEvent
        {
            Title = item.Title, LogoId = item.LogoId, VenueName = item.VenueName, Address = item.Address,
            Latitude = item.Latitude, Longitude = item.Longitude, WaitingContent = item.WaitingContent, ClosingContent = item.ClosingContent,
            DirectionsUrl = item.DirectionsUrl, Published = false, UseLiturgy = false,
        };
        ScheduleMapping.Apply(copy, normalized);
        db.Events.Add(copy);
        await db.SaveChangesAsync(cancellationToken);
        var result = await Detail(copy, cancellationToken);
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        db.OperationReceipts.Add(new() { ActorId = actorId, OperationId = operationId, Target = target,
            PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = actorId, EventId = copy.Id, SubjectId = item.Id, Action = "event-copied", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<EventSummary>> ListEvents(CancellationToken cancellationToken) =>
        await db.Events.AsNoTracking().OrderBy(x => x.Title).ThenBy(x => x.Id)
            .Select(x => new EventSummary(x.Id, x.Title, x.Published, x.UseLiturgy, Convert.ToBase64String(EF.Property<byte[]>(x, "Version")),
                x.Timezone, x.StartLocal.HasValue ? new LocalTimeInput(x.StartLocal.Value, x.StartOffsetMinutes) : null,
                x.EndLocal.HasValue ? new LocalTimeInput(x.EndLocal.Value, x.EndOffsetMinutes) : null, x.StartsAtUtc, x.EndsAtUtc))
            .ToListAsync(cancellationToken);
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Operations;
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
        var item = await db.Events.SingleOrDefaultAsync(x => x.Id == command.EventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == command.ActorId && x.EventId == command.EventId && x.OperationId == command.OperationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<EventDetail>(receipt.Result)!;
        }
        var current = Detail(item);
        if (current.Version != command.Version) throw new StaleVersionException(current);
        var input = command.Input;
        item.Title = input.Title; item.VenueName = input.VenueName; item.Address = input.Address;
        item.Latitude = input.Latitude; item.Longitude = input.Longitude;
        item.WaitingContent = input.WaitingContent; item.ClosingContent = input.ClosingContent; item.DirectionsUrl = input.DirectionsUrl;
        item.Timezone = input.Timezone; item.StartLocal = input.Start?.Local; item.EndLocal = input.End?.Local;
        item.StartOffsetMinutes = input.Start?.OffsetMinutes; item.EndOffsetMinutes = input.End?.OffsetMinutes;
        item.StartsAtUtc = input.Start?.ToUtc(); item.EndsAtUtc = input.End?.ToUtc();
        await db.SaveChangesAsync(cancellationToken);
        var result = Detail(item);
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        db.OperationReceipts.Add(new() { ActorId = command.ActorId, EventId = item.Id, OperationId = command.OperationId,
            Target = target, PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = command.ActorId, EventId = item.Id, Action = "event-saved", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private EventDetail Detail(BuildEvent item) => new(item.Id, item.Title, item.Published, item.UseLiturgy,
        Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!), item.VenueName, item.Address,
        item.Latitude, item.Longitude, item.WaitingContent, item.ClosingContent, item.DirectionsUrl, item.Timezone,
        item.StartLocal is { } start ? new(start, item.StartOffsetMinutes) : null,
        item.EndLocal is { } end ? new(end, item.EndOffsetMinutes) : null, item.StartsAtUtc, item.EndsAtUtc);

    public async Task<EventDetail?> GetEvent(Guid eventId, CancellationToken cancellationToken)
    {
        var item = await db.Events.SingleOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        return item is null ? null : Detail(item);
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

    public async Task<IReadOnlyList<EventSummary>> ListEvents(CancellationToken cancellationToken) =>
        await db.Events.AsNoTracking().OrderBy(x => x.Title).ThenBy(x => x.Id)
            .Select(x => new EventSummary(x.Id, x.Title, x.Published, x.UseLiturgy, Convert.ToBase64String(EF.Property<byte[]>(x, "Version"))))
            .ToListAsync(cancellationToken);
}

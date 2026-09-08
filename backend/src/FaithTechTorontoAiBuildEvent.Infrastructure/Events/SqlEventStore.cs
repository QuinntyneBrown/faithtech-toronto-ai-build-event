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
    public async Task<EventSummary> SaveDraft(SaveEventCommand command, CancellationToken cancellationToken)
    {
        var target = $"PUT /api/admin/events/{command.EventId}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { target, command.Title, command.Version }))));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"event:{command.EventId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        var item = await db.Events.SingleOrDefaultAsync(x => x.Id == command.EventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == command.ActorId && x.EventId == command.EventId && x.OperationId == command.OperationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<EventSummary>(receipt.Result)!;
        }
        var current = new EventSummary(item.Id, item.Title, item.Published, item.UseLiturgy, Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!));
        if (current.Version != command.Version) throw new StaleVersionException(current);
        item.Title = command.Title;
        await db.SaveChangesAsync(cancellationToken);
        var result = current with { Title = item.Title, Version = Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!) };
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        db.OperationReceipts.Add(new() { ActorId = command.ActorId, EventId = item.Id, OperationId = command.OperationId,
            Target = target, PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = command.ActorId, EventId = item.Id, Action = "event-saved", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public Task<EventSummary?> GetEvent(Guid eventId, CancellationToken cancellationToken) =>
        db.Events.AsNoTracking().Where(x => x.Id == eventId)
            .Select(x => new EventSummary(x.Id, x.Title, x.Published, x.UseLiturgy, Convert.ToBase64String(EF.Property<byte[]>(x, "Version"))))
            .SingleOrDefaultAsync(cancellationToken);

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

using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Domain.Events;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Events;

public sealed class SqlEventLogoStore(EventDbContext db, IEventStore events) : IEventLogoStore
{
    public async Task<EventDetail> Save(Guid actorId, Guid eventId, Guid operationId, string version, string payloadHash, LogoContent logo, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"event:{eventId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        var current = await events.GetEvent(eventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == actorId && x.EventId == eventId && x.OperationId == operationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.PayloadHash != payloadHash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<EventDetail>(receipt.Result)!;
        }
        if (current.Version != version) throw new StaleVersionException(current);
        var asset = new LogoAsset { Bytes = logo.Bytes, MediaType = logo.MediaType, Width = logo.Width, Height = logo.Height };
        db.Logos.Add(asset);
        var item = await db.Events.SingleAsync(x => x.Id == eventId, cancellationToken);
        item.LogoId = asset.Id;
        await db.SaveChangesAsync(cancellationToken);
        var result = await events.GetEvent(eventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        db.OperationReceipts.Add(new() { ActorId = actorId, EventId = eventId, OperationId = operationId,
            Target = $"POST /api/admin/events/{eventId}/logo", PayloadHash = payloadHash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = actorId, EventId = eventId, Action = "event-logo-saved", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public Task<LogoContent?> Get(Guid eventId, CancellationToken cancellationToken) =>
        (from item in db.Events.AsNoTracking() join logo in db.Logos on item.LogoId equals logo.Id
         where item.Id == eventId select new LogoContent(logo.Bytes, logo.MediaType, logo.Width, logo.Height)).SingleOrDefaultAsync(cancellationToken);
}

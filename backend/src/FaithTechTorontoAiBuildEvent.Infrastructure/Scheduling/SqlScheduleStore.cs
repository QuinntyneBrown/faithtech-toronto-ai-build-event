using System.Security.Cryptography;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Scheduling;

public sealed class SqlScheduleStore(EventDbContext db) : IScheduleStore
{
    public async Task<ScheduleDetail?> Get(Guid eventId, CancellationToken cancellationToken)
    {
        var item = await db.Events.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        return item is null ? null : ScheduleMapping.Detail(item, Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!), await Now(cancellationToken));
    }
    public Task<ScheduleDetail> Save(SaveScheduleCommand command, CancellationToken cancellationToken) => Save(command, false, cancellationToken);
    public Task<ScheduleDetail> ApplyReference(SaveScheduleCommand command, CancellationToken cancellationToken) => Save(command, true, cancellationToken);
    private async Task<ScheduleDetail> Save(SaveScheduleCommand command, bool reference, CancellationToken cancellationToken)
    {
        var target = reference ? $"POST /api/admin/events/{command.EventId}/reference-schedule" : $"PUT /api/admin/events/{command.EventId}/schedule";
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { target, Input = reference ? null : command.Input, command.Version })));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"event:{command.EventId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        var item = await db.Events.Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == command.EventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == command.ActorId && x.EventId == command.EventId && x.OperationId == command.OperationId, cancellationToken);
        if (receipt is not null) {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<ScheduleDetail>(receipt.Result)!;
        }
        var now = await Now(cancellationToken);
        var current = ScheduleMapping.Detail(item, Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!), now);
        if (current.Version != command.Version) throw new StaleVersionException(current);
        ScheduleClosurePolicy.CheckAndRecord(item, command.Input, now);
        var submitted = command.Input.Stages!.Select(x => x.Id).ToArray();
        if (await db.Stages.AnyAsync(x => submitted.Contains(x.Id) && x.EventId != item.Id, cancellationToken))
            throw new InputValidationException("stages", "Use fresh identities for new stages in this event.");
        var existing = item.Stages.Select(x => x.Id).ToHashSet();
        ScheduleMapping.Apply(item, command.Input);
        if (reference) { item.VenueName = "Stone Church"; item.UseLiturgy = false; }
        foreach (var stage in item.Stages.Where(x => !existing.Contains(x.Id))) db.Stages.Add(stage);
        db.Entry(item).Property(x => x.Timezone).IsModified = true;
        await db.SaveChangesAsync(cancellationToken);
        var result = ScheduleMapping.Detail(item, Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!), now);
        db.OperationReceipts.Add(new() { ActorId = command.ActorId, EventId = item.Id, OperationId = command.OperationId,
            Target = target, PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = command.ActorId, EventId = item.Id, Action = reference ? "reference-schedule-applied" : "schedule-saved", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
        return result;
    }
    private Task<DateTimeOffset> Now(CancellationToken cancellationToken) => db.Database
        .SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
}

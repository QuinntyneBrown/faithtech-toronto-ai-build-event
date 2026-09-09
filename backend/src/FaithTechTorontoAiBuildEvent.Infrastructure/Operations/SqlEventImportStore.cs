using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using FaithTechTorontoAiBuildEvent.Domain.Events;
using FaithTechTorontoAiBuildEvent.Infrastructure.Events;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using FaithTechTorontoAiBuildEvent.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

public sealed class SqlEventImportStore(EventDbContext db, OperatorPrincipal principal) : IEventImportStore
{
    private readonly SqlOperatorUnitOfWork work = new(db, principal);
    public async Task<EventImportReview> Review(Guid eventId, bool create, string? version, EventSeedEnvelope seed, CancellationToken token)
    {
        if (eventId == Guid.Empty || (create ? version is not null : string.IsNullOrWhiteSpace(version)))
            throw new InputValidationException("event", "Choose creation or an existing event with its reviewed version.");
        if ((await db.Database.GetPendingMigrationsAsync(token)).Any()) throw new InvalidOperationException("Apply the reviewed migrations first.");
        var saved = await db.Events.AsNoTracking().Include(x => x.Stages).SingleOrDefaultAsync(x => x.Id == eventId, token);
        if (create && saved is not null) throw new OperationConflictException();
        if (!create && saved is null) throw new ResourceNotFoundException();
        if (!create) {
            var currentVersion = await db.Events.Where(x => x.Id == eventId).Select(x => EF.Property<byte[]>(x, "Version")).SingleAsync(token);
            if (Convert.ToBase64String(currentVersion) != version) throw new StaleVersionException(new { EventId = eventId });
        }
        var item = saved ?? new BuildEvent { Id = eventId };
        var before = new EventSeedState(EventMapping.Input(item), ScheduleMapping.Input(item));
        var after = EventSeedMerge.Merge(seed, before.Event, before.Schedule, create);
        ScheduleClosurePolicy.CheckAndRecord(item, after.Schedule, await work.Now(token));
        if (item.Published && (after.Event.Title is null || after.Event.VenueName is null || after.Event.Address is null ||
            after.Event.Latitude is null || after.Event.Longitude is null || item.LogoId is null ||
            after.Event.Start?.ToUtc() is null || after.Event.End?.ToUtc() is null))
            throw new InputValidationException("event", "A published event must retain its required venue, branding and timing.");
        var ids = after.Schedule.Stages!.Select(x => x.Id).ToArray();
        if (await db.Stages.AnyAsync(x => ids.Contains(x.Id) && x.EventId != eventId, token))
            throw new InputValidationException("stages", "A stage identity already belongs to another event.");
        return new(eventId, create, version, seed, create ? null : before, after, item.Published);
    }
    public async Task<EventImportResult> Apply(Guid operationId, EventImportReview review, CancellationToken token)
    {
        if (operationId == Guid.Empty) throw new ArgumentException("Operation identity is required.");
        db.ChangeTracker.Clear();
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await work.Permissions(token);
        var actor = await work.Actor(token);
        var hash = SqlOperatorUnitOfWork.Hash(review);
        var receipt = await work.Receipt(operationId, hash, token);
        if (receipt is not null) return receipt;
        await work.Lock($"event:{review.EventId}", token);
        var current = await Review(review.EventId, review.Create, review.Version, review.Seed, token);
        if (SqlOperatorUnitOfWork.Hash(current) != hash) throw new OperationConflictException();
        var item = review.Create ? new BuildEvent { Id = review.EventId } : await db.Events.Include(x => x.Stages).SingleAsync(x => x.Id == review.EventId, token);
        if (review.Create) db.Events.Add(item);
        var now = await work.Now(token);
        ScheduleClosurePolicy.CheckAndRecord(item, current.After.Schedule, now);
        var existing = item.Stages.Select(x => x.Id).ToHashSet();
        EventMapping.Apply(item, current.After.Event); ScheduleMapping.Apply(item, current.After.Schedule);
        foreach (var stage in item.Stages.Where(x => !existing.Contains(x.Id))) db.Stages.Add(stage);
        if (!review.Create) db.Entry(item).Property(x => x.Timezone).IsModified = true;
        await db.SaveChangesAsync(token);
        var result = new EventImportResult(item.Id, Convert.ToBase64String(db.Entry(item).Property<byte[]>("Version").CurrentValue!), item.Published, item.Stages.Count);
        work.Record(actor, operationId, hash, result, now);
        await db.SaveChangesAsync(token); await transaction.CommitAsync(token); return result;
    }
    public async Task<EventImportResult?> Reconcile(Guid operationId, CancellationToken token)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await work.Permissions(token);
        return await work.Receipt(operationId, null, token);
    }
}

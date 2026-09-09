using System.Security.Cryptography;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Domain.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

public sealed class SqlOperatorUnitOfWork(EventDbContext db, OperatorPrincipal principal)
{
    public async Task Lock(string resource, CancellationToken token)
    {
        var timeout = (int)Math.Min((long)(db.Database.GetCommandTimeout() ?? 60) * 1000, int.MaxValue);
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout={timeout}; IF @result < 0 THROW 51000, 'Operation lock unavailable', 1;", token);
    }
    public async Task Permissions(CancellationToken token)
    {
        var allowed = await db.Database.SqlQuery<int>($"SELECT CASE WHEN HAS_PERMS_BY_NAME('dbo.Events','OBJECT','SELECT')=1 AND HAS_PERMS_BY_NAME('dbo.Events','OBJECT','INSERT')=1 AND HAS_PERMS_BY_NAME('dbo.Events','OBJECT','UPDATE')=1 AND HAS_PERMS_BY_NAME('dbo.Stages','OBJECT','SELECT')=1 AND HAS_PERMS_BY_NAME('dbo.Stages','OBJECT','INSERT')=1 AND HAS_PERMS_BY_NAME('dbo.Stages','OBJECT','UPDATE')=1 AND HAS_PERMS_BY_NAME('dbo.Stages','OBJECT','DELETE')=1 AND HAS_PERMS_BY_NAME('dbo.OperatorIdentities','OBJECT','SELECT')=1 AND HAS_PERMS_BY_NAME('dbo.OperatorIdentities','OBJECT','INSERT')=1 AND HAS_PERMS_BY_NAME('dbo.OperationReceipts','OBJECT','SELECT')=1 AND HAS_PERMS_BY_NAME('dbo.OperationReceipts','OBJECT','INSERT')=1 AND HAS_PERMS_BY_NAME('dbo.AuditRecords','OBJECT','INSERT')=1 THEN 1 ELSE 0 END AS Value").SingleAsync(token);
        if (allowed != 1) throw new UnauthorizedAccessException();
    }
    public async Task<Guid> Actor(CancellationToken token)
    {
        await Lock("operator:" + principal.Key, token);
        var actor = await db.OperatorIdentities.SingleOrDefaultAsync(x => x.PrincipalKey == principal.Key, token);
        if (actor is not null) return actor.Id;
        actor = new() { PrincipalKey = principal.Key }; db.OperatorIdentities.Add(actor);
        await db.SaveChangesAsync(token); return actor.Id;
    }
    public async Task<EventImportResult?> Receipt(Guid operation, string? hash, CancellationToken token)
    {
        await Lock($"operator:{principal.Key}:{operation}", token);
        var receipt = await (from row in db.OperationReceipts.IgnoreQueryFilters()
            join actor in db.OperatorIdentities on row.ActorId equals actor.Id
            where actor.PrincipalKey == principal.Key && row.ActorKind == ActorKind.DatabaseOperator && row.OperationId == operation
            select row).SingleOrDefaultAsync(token);
        if (receipt is null) return null;
        if (hash is not null && receipt.PayloadHash != hash) throw new OperationConflictException();
        return JsonSerializer.Deserialize<EventImportResult>(receipt.Result)!;
    }
    public void Record(Guid actor, Guid operation, string hash, EventImportResult result, DateTimeOffset now)
    {
        db.OperationReceipts.Add(new() { ActorId = actor, ActorKind = ActorKind.DatabaseOperator, OperationId = operation,
            Target = "events import", PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = actor, ActorKind = ActorKind.DatabaseOperator, EventId = result.EventId,
            SubjectId = operation, Action = "event-imported", Outcome = "succeeded", AtUtc = now });
    }
    public Task<DateTimeOffset> Now(CancellationToken token) => db.Database
        .SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(token);
    public static string Hash(EventImportReview review) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(
        new { review.EventId, review.Create, review.Version, review.After })));
}

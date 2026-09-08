using System.Security.Cryptography;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Roster;
using FaithTechTorontoAiBuildEvent.Domain.Roster;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Roster;

public sealed class SqlRosterStore(EventDbContext db, IEntryCodeGenerator codes, IParticipantStore participants) : IRosterStore
{
    public async Task<IReadOnlyList<RosterEntry>> List(Guid eventId, CancellationToken cancellationToken)
    {
        if (!await db.Events.AnyAsync(x => x.Id == eventId, cancellationToken)) throw new ResourceNotFoundException();
        var entries = await db.Registrations.Where(x => x.EventId == eventId).OrderBy(x => x.DisplayName).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        return entries.Select(Detail).ToArray();
    }
    public async Task<RosterIssuance> Add(AddRegistrationCommand command, CancellationToken cancellationToken)
    {
        var target = $"POST /api/admin/events/{command.EventId}/roster";
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { target, command.Input })));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"event:{command.EventId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        if (!await db.Events.AnyAsync(x => x.Id == command.EventId, cancellationToken)) throw new ResourceNotFoundException();
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == command.ActorId && x.EventId == command.EventId && x.OperationId == command.OperationId, cancellationToken);
        if (receipt is not null) {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<RosterIssuance>(receipt.Result)!;
        }
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        var entry = new Registration { EventId = command.EventId, DisplayName = command.Input.DisplayName!, CodeIssuedAtUtc = now };
        string code = "";
        for (var attempt = 0; attempt < 5; attempt++) {
            code = codes.Generate(); entry.CodeDigest = codes.Digest(command.EventId, code);
            if (!await db.Registrations.AnyAsync(x => x.EventId == command.EventId && x.CodeDigest == entry.CodeDigest, cancellationToken)) break;
            if (attempt == 4) throw new InvalidOperationException("A unique entry credential could not be generated.");
        }
        db.Registrations.Add(entry); await db.SaveChangesAsync(cancellationToken);
        var result = new RosterIssuance(Detail(entry), code, false, entry.CredentialVersion);
        db.OperationReceipts.Add(new() { ActorId = command.ActorId, EventId = command.EventId, OperationId = command.OperationId, Target = target,
            PayloadHash = hash, Result = JsonSerializer.Serialize(result with { Code = null, PreviouslyCompleted = true }), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = command.ActorId, EventId = command.EventId, SubjectId = entry.Id, Action = "registration-added", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); return result;
    }
    public async Task<RosterEntry> Rename(RenameRegistrationCommand command, CancellationToken cancellationToken)
    {
        var target = $"PUT /api/admin/events/{command.EventId}/roster/{command.RegistrationId}/name";
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { target, command.DisplayName, command.Version })));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"event:{command.EventId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        var entry = await db.Registrations.SingleOrDefaultAsync(x => x.Id == command.RegistrationId && x.EventId == command.EventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == command.ActorId && x.EventId == command.EventId && x.OperationId == command.OperationId, cancellationToken);
        if (receipt is not null) {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<RosterEntry>(receipt.Result)!;
        }
        var current = Detail(entry);
        if (current.Version != command.Version) throw new StaleVersionException(current);
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        entry.DisplayName = command.DisplayName!;
        await db.SaveChangesAsync(cancellationToken);
        var result = Detail(entry);
        db.OperationReceipts.Add(new() { ActorId = command.ActorId, EventId = command.EventId, OperationId = command.OperationId, Target = target,
            PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = command.ActorId, EventId = command.EventId, SubjectId = entry.Id, Action = "registration-renamed", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); return result;
    }
    public async Task<RosterEntry> Deactivate(DeactivateRegistrationCommand command, CancellationToken cancellationToken)
    {
        var target = $"POST /api/admin/events/{command.EventId}/roster/{command.RegistrationId}/deactivate";
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { target, command.Version })));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var resource = $"event:{command.EventId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Operation unavailable', 1;", cancellationToken);
        var entry = await db.Registrations.SingleOrDefaultAsync(x => x.Id == command.RegistrationId && x.EventId == command.EventId, cancellationToken) ?? throw new ResourceNotFoundException();
        var receipt = await db.OperationReceipts.SingleOrDefaultAsync(x => x.ActorId == command.ActorId && x.EventId == command.EventId && x.OperationId == command.OperationId, cancellationToken);
        if (receipt is not null) {
            if (receipt.PayloadHash != hash) throw new OperationConflictException();
            return JsonSerializer.Deserialize<RosterEntry>(receipt.Result)!;
        }
        var current = Detail(entry);
        if (current.Version != command.Version) throw new StaleVersionException(current);
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        entry.Active = false;
        await db.SaveChangesAsync(cancellationToken);
        await participants.RevokeSessionsForRegistration(entry.Id, cancellationToken);
        var result = Detail(entry);
        db.OperationReceipts.Add(new() { ActorId = command.ActorId, EventId = command.EventId, OperationId = command.OperationId, Target = target,
            PayloadHash = hash, Result = JsonSerializer.Serialize(result), CommittedAtUtc = now });
        db.AuditRecords.Add(new() { ActorId = command.ActorId, EventId = command.EventId, SubjectId = entry.Id, Action = "registration-deactivated", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); return result;
    }

    private RosterEntry Detail(Registration entry) => new(entry.Id, entry.DisplayName, entry.Active, entry.NormalizedEmail is not null,
        entry.FirstAccessAtUtc, Convert.ToBase64String(db.Entry(entry).Property<byte[]>("Version").CurrentValue!));
}

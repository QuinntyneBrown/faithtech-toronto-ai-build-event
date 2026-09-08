using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Roster;
using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SqlParticipantStore(EventDbContext db, IEntryCodeGenerator codes, AuthenticationBudget budget) : IParticipantStore
{
    public async Task<ParticipantSession?> Authenticate(Guid eventId, string email, string normalizedEmail, string entryCode, string source, CancellationToken cancellationToken)
    {
        var codeDigest = codes.Digest(eventId, entryCode);
        var registrationId = await budget.Verify($"participant:{eventId:N}:{entryCode.Trim().ToUpperInvariant()}", source,
            () => BindEmail(eventId, codeDigest, email, normalizedEmail, cancellationToken), cancellationToken);
        if (registrationId is null) return null;
        var now = await GetUtcNow(cancellationToken);
        var session = new ParticipantSession { RegistrationId = registrationId.Value, EventId = eventId, AuthenticatedAtUtc = now };
        db.ParticipantSessions.Add(session);
        db.AuditRecords.Add(new() { ActorId = registrationId.Value, EventId = eventId, SubjectId = registrationId.Value,
            Action = "participant-authenticated", Outcome = "succeeded", AtUtc = now });
        await db.SaveChangesAsync(cancellationToken);
        return session;
    }

    /// <summary>Verifies the code and atomically binds or resumes an email. Same-code races are already serialized by the
    /// caller's authentication budget lock (both share one account key); the unique filtered index on (EventId, NormalizedEmail)
    /// remains the source of truth for cross-code races, caught here as a denial rather than an exception.</summary>
    private async Task<Guid?> BindEmail(Guid eventId, string codeDigest, string email, string normalizedEmail, CancellationToken cancellationToken)
    {
        if (!await db.Events.AnyAsync(x => x.Id == eventId && x.Published, cancellationToken)) return null;
        var entry = await db.Registrations.SingleOrDefaultAsync(x => x.EventId == eventId && x.CodeDigest == codeDigest && x.Active, cancellationToken);
        if (entry is null) return null;
        if (entry.NormalizedEmail is not null) return entry.NormalizedEmail == normalizedEmail ? entry.Id : null;
        if (await db.Registrations.AnyAsync(x => x.EventId == eventId && x.Active && x.NormalizedEmail == normalizedEmail, cancellationToken)) return null;
        entry.Email = email; entry.NormalizedEmail = normalizedEmail;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { db.Entry(entry).State = EntityState.Unchanged; return null; }
        return entry.Id;
    }

    public Task<ParticipantSession?> FindSession(Guid sessionId, CancellationToken cancellationToken) =>
        db.ParticipantSessions.SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

    public Task<DateTimeOffset> GetUtcNow(CancellationToken cancellationToken) =>
        db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);

    public async Task<bool> IsActiveRegistration(Guid registrationId, CancellationToken cancellationToken) =>
        await db.Registrations.AnyAsync(x => x.Id == registrationId && x.Active, cancellationToken);
}

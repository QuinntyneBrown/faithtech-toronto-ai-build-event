using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Domain.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class SqlPublicEntryAttemptStore(CompanionDbContext database) : IPublicEntryAttemptStore
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public async Task<TimeSpan?> GetRetryAfterAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var attempts = database.PublicEntryAttempts.Where(attempt => attempt.Source == source && attempt.AttemptedAtUtc > nowUtc - Window);
        if (await attempts.CountAsync(cancellationToken) < 60) return null;
        return (await attempts.OrderBy(attempt => attempt.AttemptedAtUtc).Select(attempt => attempt.AttemptedAtUtc).FirstAsync(cancellationToken)).Add(Window) - nowUtc;
    }

    public async Task RecordAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        database.PublicEntryAttempts.Add(new PublicEntryAttempt { Id = Guid.NewGuid(), Source = source, AttemptedAtUtc = nowUtc });
        await database.SaveChangesAsync(cancellationToken);
    }
}

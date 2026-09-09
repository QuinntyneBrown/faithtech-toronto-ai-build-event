using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SqlAdministratorLoginAttemptStore(CompanionDbContext database) : IAdministratorLoginAttemptStore
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    public async Task<TimeSpan?> GetRetryAfterAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var oldest = await database.AdministratorLoginAttempts.Where(attempt => attempt.Source == source && attempt.AttemptedAtUtc > nowUtc - Window).OrderBy(attempt => attempt.AttemptedAtUtc).Select(attempt => (DateTimeOffset?)attempt.AttemptedAtUtc).FirstOrDefaultAsync(cancellationToken);
        var count = await database.AdministratorLoginAttempts.CountAsync(attempt => attempt.Source == source && attempt.AttemptedAtUtc > nowUtc - Window, cancellationToken);
        return count < 5 || oldest is null ? null : oldest.Value.Add(Window) - nowUtc;
    }

    public async Task RecordFailureAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        database.AdministratorLoginAttempts.Add(new AdministratorLoginAttempt { Id = Guid.NewGuid(), Source = source, AttemptedAtUtc = nowUtc });
        await database.SaveChangesAsync(cancellationToken);
    }
}

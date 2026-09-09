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
        var recent = database.AdministratorLoginAttempts.Where(attempt => attempt.AttemptedAtUtc > nowUtc - Window);
        var sourceAttempts = recent.Where(attempt => attempt.Source == source);
        var sourceCount = await sourceAttempts.CountAsync(cancellationToken);
        var deploymentCount = await recent.CountAsync(cancellationToken);
        if (sourceCount < 5 && deploymentCount < 20) return null;

        var limitingAttempts = sourceCount >= 5 ? sourceAttempts : recent;
        var oldest = await limitingAttempts.OrderBy(attempt => attempt.AttemptedAtUtc).Select(attempt => attempt.AttemptedAtUtc).FirstAsync(cancellationToken);
        return oldest.Add(Window) - nowUtc;
    }

    public async Task RecordFailureAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        database.AdministratorLoginAttempts.Add(new AdministratorLoginAttempt { Id = Guid.NewGuid(), Source = source, AttemptedAtUtc = nowUtc });
        await database.SaveChangesAsync(cancellationToken);
    }
}

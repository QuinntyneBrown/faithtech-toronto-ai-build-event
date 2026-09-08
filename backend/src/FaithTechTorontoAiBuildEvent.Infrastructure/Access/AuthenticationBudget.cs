using System.Security.Cryptography;
using System.Text;
using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class AuthenticationBudget(EventDbContext db, IOptions<SecurityOptions> options)
{
    public async Task<Guid?> Verify(string accountScope, string source, Func<Task<Guid?>> verify, CancellationToken cancellationToken)
    {
        var key = Convert.FromBase64String(options.Value.DigestKey);
        string Digest(string value) => Convert.ToHexString(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(value)));
        var accountKey = Digest(accountScope);
        var sourceKey = Digest("source:" + source);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        foreach (var resource in new[] { accountKey, sourceKey }.Order(StringComparer.Ordinal))
            await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=5000; IF @result < 0 THROW 51000, 'Authentication budget unavailable', 1;", cancellationToken);
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync(cancellationToken);
        var failures = await db.AuthenticationFailures.Where(x => x.FailedAtUtc > now.AddMinutes(-5) &&
            (x.SourceKey == sourceKey || x.AccountKey == accountKey)).ToListAsync(cancellationToken);
        var account = failures.Where(x => x.AccountKey == accountKey).OrderBy(x => x.FailedAtUtc).ToList();
        var address = failures.Where(x => x.SourceKey == sourceKey).OrderBy(x => x.FailedAtUtc).ToList();
        var retryAt = now;
        if (account.Count >= 5) retryAt = account[account.Count - 5].FailedAtUtc.AddMinutes(5);
        if (address.Count >= 10 && address[address.Count - 10].FailedAtUtc.AddMinutes(5) > retryAt)
            retryAt = address[address.Count - 10].FailedAtUtc.AddMinutes(5);
        if (retryAt > now) throw new AuthenticationThrottledException(Math.Max(1, (int)Math.Ceiling((retryAt - now).TotalSeconds)));
        var actorId = await verify();
        if (actorId is null)
        {
            db.AuthenticationFailures.Add(new() { SourceKey = sourceKey, AccountKey = accountKey, FailedAtUtc = now });
            await db.SaveChangesAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        return actorId;
    }
}

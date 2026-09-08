using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SqlAdministratorProvisioner(EventDbContext db, UserManager<AdministratorAccount> users,
    RoleManager<IdentityRole<Guid>> roles) : IAdministratorProvisioner
{
    public async Task<Guid> Provision(string username, string password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length > 256 || string.IsNullOrEmpty(password) || password.Length > 1024)
            throw new InvalidOperationException("Invalid administrator input.");
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "EXEC sp_getapplock @Resource='administrator-provisioning', @LockMode='Exclusive', @LockOwner='Transaction'", cancellationToken);
        if (!await roles.RoleExistsAsync("Administrator"))
            RequireSuccess(await roles.CreateAsync(new IdentityRole<Guid>("Administrator")));
        var account = new AdministratorAccount { Id = Guid.NewGuid(), UserName = username };
        RequireSuccess(await users.CreateAsync(account, password));
        RequireSuccess(await users.AddToRoleAsync(account, "Administrator"));
        await transaction.CommitAsync(cancellationToken);
        return account.Id;
    }

    public async Task Disable(string username, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var account = await users.FindByNameAsync(username) ?? throw new InvalidOperationException("Administrator not found.");
        account.Enabled = false;
        RequireSuccess(await users.UpdateAsync(account));
        await db.AdministratorSessions.Where(x => x.AdministratorId == account.Id)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.Revoked, true), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<string[]> ResetPasswords(string? username, string password, CancellationToken cancellationToken)
    {
        if (username is not null && (string.IsNullOrWhiteSpace(username) || username.Length > 256) ||
            string.IsNullOrEmpty(password) || password.Length > 1024)
            throw new InvalidOperationException("Invalid administrator input.");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var normalized = username is null ? null : users.NormalizeName(username);
        var accounts = await db.Users.Where(x => normalized == null || x.NormalizedUserName == normalized)
            .OrderBy(x => x.UserName).ToArrayAsync(cancellationToken);
        if (username is not null && accounts.Length == 0) throw new InvalidOperationException("Administrator not found.");
        foreach (var account in accounts.Length == 0 ? [new AdministratorAccount()] : accounts)
            foreach (var validator in users.PasswordValidators)
                RequireSuccess(await validator.ValidateAsync(users, account, password));
        foreach (var account in accounts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            account.PasswordHash = users.PasswordHasher.HashPassword(account, password);
            account.SecurityStamp = Guid.NewGuid().ToString();
            RequireSuccess(await users.UpdateAsync(account));
            await db.AdministratorSessions.Where(x => x.AdministratorId == account.Id)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.Revoked, true), cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
        return accounts.Select(x => x.UserName!).ToArray();
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Code)));
    }
}

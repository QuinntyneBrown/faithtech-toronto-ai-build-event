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

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Code)));
    }
}

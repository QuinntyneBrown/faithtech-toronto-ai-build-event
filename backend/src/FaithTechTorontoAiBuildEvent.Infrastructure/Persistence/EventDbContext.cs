using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

public sealed class EventDbContext(DbContextOptions<EventDbContext> options)
    : IdentityDbContext<AdministratorAccount, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<AdministratorSession> AdministratorSessions => Set<AdministratorSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AdministratorSession>().HasOne<AdministratorAccount>().WithMany()
            .HasForeignKey(x => x.AdministratorId).OnDelete(DeleteBehavior.Restrict);
    }
}

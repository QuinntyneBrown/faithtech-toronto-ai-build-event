using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Domain.Events;
using FaithTechTorontoAiBuildEvent.Domain.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

public sealed class EventDbContext(DbContextOptions<EventDbContext> options)
    : IdentityDbContext<AdministratorAccount, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<AdministratorSession> AdministratorSessions => Set<AdministratorSession>();
    public DbSet<AuthenticationFailure> AuthenticationFailures => Set<AuthenticationFailure>();
    public DbSet<BuildEvent> Events => Set<BuildEvent>();
    public DbSet<OperationReceipt> OperationReceipts => Set<OperationReceipt>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<BuildEvent>().Property(x => x.Title).HasMaxLength(400);
        builder.Entity<BuildEvent>().Property(x => x.Timezone).HasMaxLength(400);
        builder.Entity<BuildEvent>().Property(x => x.VenueName).HasMaxLength(400);
        builder.Entity<BuildEvent>().Property(x => x.Address).HasMaxLength(10000);
        builder.Entity<BuildEvent>().Property(x => x.WaitingContent).HasMaxLength(10000);
        builder.Entity<BuildEvent>().Property(x => x.ClosingContent).HasMaxLength(10000);
        builder.Entity<BuildEvent>().Property(x => x.DirectionsUrl).HasMaxLength(4096);
        builder.Entity<BuildEvent>().Property<byte[]>("Version").IsRowVersion();
        builder.Entity<OperationReceipt>().HasIndex(x => new { x.ActorId, x.EventId, x.OperationId }).IsUnique().HasFilter(null);
        builder.Entity<OperationReceipt>().Property(x => x.Target).HasMaxLength(200);
        builder.Entity<OperationReceipt>().Property(x => x.PayloadHash).HasMaxLength(64);
        builder.Entity<AuditRecord>().Property(x => x.Action).HasMaxLength(100);
        builder.Entity<AuditRecord>().Property(x => x.Outcome).HasMaxLength(50);
        builder.Entity<AuthenticationFailure>().Property(x => x.SourceKey).HasMaxLength(64);
        builder.Entity<AuthenticationFailure>().Property(x => x.AccountKey).HasMaxLength(64);
        builder.Entity<AuthenticationFailure>().HasIndex(x => new { x.SourceKey, x.FailedAtUtc });
        builder.Entity<AuthenticationFailure>().HasIndex(x => new { x.AccountKey, x.FailedAtUtc });
        builder.Entity<AdministratorSession>().HasOne<AdministratorAccount>().WithMany()
            .HasForeignKey(x => x.AdministratorId).OnDelete(DeleteBehavior.Restrict);
    }
}

using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Domain.Events;
using FaithTechTorontoAiBuildEvent.Domain.Operations;
using FaithTechTorontoAiBuildEvent.Domain.Scheduling;
using FaithTechTorontoAiBuildEvent.Domain.Roster;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

public sealed class EventDbContext(DbContextOptions<EventDbContext> options)
    : IdentityDbContext<AdministratorAccount, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<AdministratorSession> AdministratorSessions => Set<AdministratorSession>();
    public DbSet<ParticipantSession> ParticipantSessions => Set<ParticipantSession>();
    public DbSet<AuthenticationFailure> AuthenticationFailures => Set<AuthenticationFailure>();
    public DbSet<BuildEvent> Events => Set<BuildEvent>();
    public DbSet<LogoAsset> Logos => Set<LogoAsset>();
    public DbSet<EventStage> Stages => Set<EventStage>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<OperationReceipt> OperationReceipts => Set<OperationReceipt>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Registration>().HasOne<BuildEvent>().WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Registration>().Property(x => x.DisplayName).HasMaxLength(400);
        builder.Entity<Registration>().Property(x => x.Email).HasMaxLength(640);
        builder.Entity<Registration>().Property(x => x.NormalizedEmail).HasMaxLength(640);
        builder.Entity<Registration>().Property(x => x.CodeDigest).HasMaxLength(64);
        builder.Entity<Registration>().Property<byte[]>("Version").IsRowVersion();
        builder.Entity<Registration>().HasIndex(x => new { x.EventId, x.CodeDigest }).IsUnique();
        builder.Entity<Registration>().HasIndex(x => new { x.EventId, x.NormalizedEmail }).IsUnique().HasFilter("[Active] = 1 AND [NormalizedEmail] IS NOT NULL");
        builder.Entity<BuildEvent>().Property(x => x.Title).HasMaxLength(400);
        builder.Entity<BuildEvent>().HasOne<LogoAsset>().WithMany().HasForeignKey(x => x.LogoId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<LogoAsset>().Property(x => x.MediaType).HasMaxLength(50);
        builder.Entity<BuildEvent>().Property(x => x.Timezone).HasMaxLength(400);
        builder.Entity<BuildEvent>().Property(x => x.VenueName).HasMaxLength(400);
        builder.Entity<BuildEvent>().Property(x => x.Address).HasMaxLength(10000);
        builder.Entity<BuildEvent>().Property(x => x.WaitingContent).HasMaxLength(10000);
        builder.Entity<BuildEvent>().Property(x => x.ClosingContent).HasMaxLength(10000);
        builder.Entity<BuildEvent>().Property(x => x.DirectionsUrl).HasMaxLength(4096);
        builder.Entity<BuildEvent>().Property<byte[]>("Version").IsRowVersion();
        builder.Entity<BuildEvent>().OwnsOne(x => x.SelectionWindow);
        builder.Entity<BuildEvent>().OwnsOne(x => x.PresentationWindow);
        builder.Entity<BuildEvent>().HasMany(x => x.Stages).WithOne().HasForeignKey(x => x.EventId);
        builder.Entity<EventStage>().OwnsOne(x => x.Interval);
        builder.Entity<EventStage>().Property(x => x.Name).HasMaxLength(400);
        builder.Entity<EventStage>().Property(x => x.Phase).HasMaxLength(400);
        builder.Entity<EventStage>().Property(x => x.ScreenType).HasMaxLength(50);
        builder.Entity<EventStage>().Property(x => x.Content).HasMaxLength(10000);
        builder.Entity<EventStage>().Property(x => x.ResourceUrl).HasMaxLength(4096);
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
        builder.Entity<ParticipantSession>().HasOne<Registration>().WithMany()
            .HasForeignKey(x => x.RegistrationId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ParticipantSession>().HasOne<BuildEvent>().WithMany()
            .HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
    }
}

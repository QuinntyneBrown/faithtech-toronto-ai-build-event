using FaithTechTorontoAiBuildEvent.Domain.Projects;
using FaithTechTorontoAiBuildEvent.Domain.Participants;
using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Domain.Teams;
using Microsoft.EntityFrameworkCore;
using DomainEventState = FaithTechTorontoAiBuildEvent.Domain.EventFlow.EventState;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

public sealed class CompanionDbContext(DbContextOptions<CompanionDbContext> options) : DbContext(options)
{
    public DbSet<DomainEventState> EventStates => Set<DomainEventState>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<EntryReceipt> EntryReceipts => Set<EntryReceipt>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<ParticipantSession> ParticipantSessions => Set<ParticipantSession>();
    public DbSet<CompanionCredential> CompanionCredentials => Set<CompanionCredential>();
    public DbSet<AdministratorSession> AdministratorSessions => Set<AdministratorSession>();
    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DomainEventState>(builder =>
        {
            builder.HasKey(state => state.Id);
            builder.Property(state => state.Version).IsRequired();
            builder.Property(state => state.CurrentScreen).HasConversion<string>().HasMaxLength(16);
        });

        modelBuilder.Entity<Project>(builder =>
        {
            builder.HasKey(project => project.Id);
            builder.Property(project => project.Title).HasMaxLength(400).IsRequired();
            builder.Property(project => project.Description).HasMaxLength(4000).IsRequired();
            builder.Property(project => project.RepositoryUrl).HasMaxLength(4096);
            builder.Property(project => project.DemoUrl).HasMaxLength(4096);
        });

        modelBuilder.Entity<EntryReceipt>(builder =>
        {
            builder.HasKey(receipt => receipt.Id);
            builder.Property(receipt => receipt.SecretDigest).HasMaxLength(32).IsRequired();
            builder.HasIndex(receipt => receipt.ExpiresAtUtc);
        });

        modelBuilder.Entity<Participant>(builder =>
        {
            builder.HasKey(participant => participant.Id);
            builder.Property(participant => participant.Email).HasMaxLength(508).IsRequired();
            builder.Property(participant => participant.NormalizedEmail).HasMaxLength(508).IsRequired();
            builder.Property(participant => participant.PublicLabel).HasMaxLength(64).IsRequired();
            builder.HasIndex(participant => participant.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<Team>(builder =>
        {
            builder.HasKey(team => team.Id);
            builder.Property(team => team.Label).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<ParticipantSession>(builder =>
        {
            builder.HasKey(session => session.Id);
            builder.Property(session => session.SecretDigest).HasMaxLength(32).IsRequired();
            builder.HasIndex(session => session.ExpiresAtUtc);
        });

        modelBuilder.Entity<CompanionCredential>(builder =>
        {
            builder.HasKey(credential => credential.Id);
            builder.Property(credential => credential.Salt).HasMaxLength(32).IsRequired();
            builder.Property(credential => credential.Verifier).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<AdministratorSession>(builder =>
        {
            builder.HasKey(session => session.Id);
            builder.Property(session => session.SecretDigest).HasMaxLength(32).IsRequired();
            builder.HasIndex(session => session.CreatedAtUtc);
        });
    }
}

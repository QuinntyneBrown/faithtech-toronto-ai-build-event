using FaithTechTorontoAiBuildEvent.Application.EventState;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.EventState;
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using FaithTechTorontoAiBuildEvent.Infrastructure.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using FaithTechTorontoAiBuildEvent.Application.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCompanionInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventOptions>(configuration.GetSection(EventOptions.SectionName));
        var connectionString = configuration.GetConnectionString("Companion")
            ?? throw new InvalidOperationException("ConnectionStrings:Companion is required.");
        services.AddDbContext<CompanionDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IEventStateStore, SqlEventStateStore>();
        services.AddScoped<IReadinessStore, SqlReadinessStore>();
        services.AddScoped<IEntryReceiptStore, SqlEntryReceiptStore>();
        services.AddScoped<IEntryStore, SqlEntryStore>();
        services.AddScoped<IParticipantSessionStore, SqlParticipantSessionStore>();
        services.AddScoped<IProfileStore, SqlProfileStore>();
        services.AddSingleton<IEntryReceiptSecretService, EntryReceiptSecretService>();
        services.AddSingleton<PasscodeVerifier>();
        services.AddScoped<IAdministratorCredentialProvisioner, SqlAdministratorCredentialProvisioner>();
        services.AddScoped<IAdministratorSessionStore, SqlAdministratorSessionStore>();
        services.AddScoped<IAdministratorAuthorizationStore, SqlAdministratorAuthorizationStore>();
        services.AddHostedService<EventStateInitializer>();
        return services;
    }
}

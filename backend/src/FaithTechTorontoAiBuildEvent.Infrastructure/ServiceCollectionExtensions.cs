using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Infrastructure.Events;
using FaithTechTorontoAiBuildEvent.Infrastructure.Scheduling;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Application.Roster;
using FaithTechTorontoAiBuildEvent.Infrastructure.Roster;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FaithTechTorontoAiBuildEvent.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(options => options.RegisterServicesFromAssemblyContaining<AuthenticateAdministratorCommand>());
        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection("ConnectionStrings"))
            .Validate(options => options.IsValid(), "Configure ConnectionStrings:EventDatabase with a server, database and MARS disabled.").ValidateOnStart();
        services.AddDbContext<EventDbContext>((provider, options) => options.UseSqlServer(
            provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.EventDatabase));
        services.AddIdentityCore<AdministratorAccount>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<EventDbContext>();
        services.AddScoped<IAdministratorStore, SqlAdministratorStore>();
        services.AddScoped<IEventStore, SqlEventStore>();
        services.AddScoped<IScheduleStore, SqlScheduleStore>();
        services.AddScoped<IRosterStore, SqlRosterStore>();
        services.AddSingleton<IEntryCodeGenerator, EntryCodeGenerator>();
        services.AddScoped<IEventLogoStore, SqlEventLogoStore>();
        services.AddSingleton<ILogoDecoder, SkiaLogoDecoder>();
        services.AddScoped<IAdministratorProvisioner, SqlAdministratorProvisioner>();
        services.AddScoped<AuthenticationBudget>();
        services.AddScoped<IRequestSource, OperatorRequestSource>();
        services.AddOptions<SecurityOptions>().Bind(configuration.GetSection("Security"))
            .Validate(options => options.HasValidDigestKey(), "Configure Security:DigestKey with at least 32 random bytes encoded as base64.");
        return services;
    }
}

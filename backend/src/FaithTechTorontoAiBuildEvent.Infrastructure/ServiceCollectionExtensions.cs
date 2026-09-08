using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(options => options.RegisterServicesFromAssemblyContaining<AuthenticateAdministratorCommand>());
        services.AddDbContext<EventDbContext>(options => options.UseSqlServer(
            configuration.GetConnectionString("EventDatabase") ?? throw new InvalidOperationException("Configure ConnectionStrings:EventDatabase.")));
        services.AddIdentityCore<AdministratorAccount>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<EventDbContext>();
        services.AddScoped<IAdministratorStore, SqlAdministratorStore>();
        services.AddScoped<IAdministratorProvisioner, SqlAdministratorProvisioner>();
        return services;
    }
}

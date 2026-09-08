using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace FaithTechTorontoAiBuildEvent.Api.Hosting;

public static class HostingConfiguration
{
    public static IServiceCollection AddEventHosting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            foreach (var address in configuration.GetSection("Hosting:KnownProxies").Get<string[]>() ?? [])
                options.KnownProxies.Add(IPAddress.Parse(address));
            foreach (var network in configuration.GetSection("Hosting:KnownNetworks").Get<string[]>() ?? [])
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        });
        return services;
    }
}

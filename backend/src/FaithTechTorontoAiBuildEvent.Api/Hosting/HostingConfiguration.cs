using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;

namespace FaithTechTorontoAiBuildEvent.Api.Hosting;

public static class HostingConfiguration
{
    public static IServiceCollection AddEventHosting(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["Hosting:KeyDirectory"] is { Length: > 0 } keyDirectory)
        {
            var certificatePath = configuration["Hosting:KeyCertificatePath"]
                ?? throw new InvalidOperationException("Configure Hosting:KeyCertificatePath for encrypted persistent keys.");
            var certificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath,
                configuration["Hosting:KeyCertificatePassword"], X509KeyStorageFlags.EphemeralKeySet);
            services.AddDataProtection().SetApplicationName("FaithTechTorontoAiBuildEvent")
                .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory)).ProtectKeysWithCertificate(certificate);
        }
        else if (!string.IsNullOrEmpty(configuration["WEBSITE_SITE_NAME"]))
            throw new InvalidOperationException("Configure persistent encrypted keys before starting on App Service.");
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

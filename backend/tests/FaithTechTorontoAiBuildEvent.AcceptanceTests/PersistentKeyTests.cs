using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class PersistentKeyTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-045")]
    public void Given_persistent_keys_when_a_second_host_starts_then_it_can_read_the_first_hosts_protected_data()
    {
        var directory = Path.Combine(Path.GetTempPath(), "FaithTechKeys", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=FaithTech test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
        var certificatePath = Path.Combine(directory, "key.pfx");
        File.WriteAllBytes(certificatePath, certificate.Export(X509ContentType.Pfx, "test-only"));
        try
        {
            void Configure(IWebHostBuilder builder) => builder
                .UseSetting("Hosting:KeyDirectory", directory)
                .UseSetting("Hosting:KeyCertificatePath", certificatePath)
                .UseSetting("Hosting:KeyCertificatePassword", "test-only");
            string protectedValue;
            using (var first = factory.WithWebHostBuilder(Configure))
                protectedValue = first.Services.GetRequiredService<IDataProtectionProvider>()
                    .CreateProtector("restart-test").Protect("session survives");
            Assert.NotEmpty(Directory.GetFiles(directory, "key-*.xml"));
            using var second = factory.WithWebHostBuilder(Configure);
            Assert.Equal("session survives", second.Services.GetRequiredService<IDataProtectionProvider>()
                .CreateProtector("restart-test").Unprotect(protectedValue));
            Assert.All(Directory.GetFiles(directory, "key-*.xml"), file => Assert.Contains("encryptedSecret", File.ReadAllText(file)));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}

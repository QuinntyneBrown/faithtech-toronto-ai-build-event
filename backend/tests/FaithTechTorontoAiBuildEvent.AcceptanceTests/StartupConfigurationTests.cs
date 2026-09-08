using FaithTechTorontoAiBuildEvent.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class StartupConfigurationTests
{
    [Fact, Trait("Requirement", "L2-045")]
    public void Given_missing_database_configuration_when_starting_then_the_application_fails_before_accepting_requests()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:EventDatabase", "");
            builder.UseSetting("Security:DigestKey", Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));
        });
        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }
}

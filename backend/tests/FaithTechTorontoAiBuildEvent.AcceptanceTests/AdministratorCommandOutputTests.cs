// Acceptance Test
// Traces to: L2-061, L2-062
// Description: CLI identity, protected output, and syntax failures are observable process behavior.
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorCommandOutputTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-062/AC6")]
    public async Task Given_a_closed_output_pipe_when_reset_commits_then_the_result_reports_delivery_failure()
    {
        var name = await factory.ProvisionAdministrator("previous2026!password");
        var result = await ProvisioningProcess.Execute(factory, ["reset-password", name, "--password-stdin"],
            "replacement2026!password", closeOutputBeforeMutation: true);
        Assert.Equal(7, result.ExitCode);
        Assert.Contains("committed", result.Error);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Infrastructure.Access.AdministratorAccount>>();
        var account = await users.FindByNameAsync(name);
        Assert.True(await users.CheckPasswordAsync(account!, "replacement2026!password"));
    }

    [Fact, Trait("Requirement", "L2-061/AC2;L2-062/AC6")]
    public async Task Given_custom_passwords_when_commands_succeed_then_the_target_and_result_exclude_secrets()
    {
        var username = $"output-{Guid.NewGuid():N}";
        var password = $"custom2026!{Guid.NewGuid():N}";
        var created = await ProvisioningProcess.Execute(factory, ["add-user", username, "--password-stdin"], password);
        Assert.Equal(0, created.ExitCode);
        Assert.Contains("Database:", created.Output);
        Assert.Contains("Principal:", created.Output);
        var reset = await ProvisioningProcess.Execute(factory, ["reset-password", username, "--password-stdin"], password);
        Assert.Equal(0, reset.ExitCode);
        Assert.Contains(username, reset.Output);
        Assert.Contains("passwords: 1", reset.Output);
        Assert.DoesNotContain(password, created.Output + created.Error + reset.Output + reset.Error);
    }

    [Fact, Trait("Requirement", "L2-062/AC3;L2-062/AC6")]
    public async Task Given_a_database_failure_when_reset_then_diagnostics_are_redacted()
    {
        var name = await factory.ProvisionAdministrator("previous2026!password");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var diagnostic = "synthetic-private-value";
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER RejectReset ON dbo.AspNetUsers AFTER UPDATE AS BEGIN THROW 51000, 'synthetic-private-value', 1; END");
        try
        {
            var result = await ProvisioningProcess.Execute(factory, ["reset-password", name, "--password-stdin"], "custom2026!password");
            Assert.NotEqual(0, result.ExitCode);
            Assert.DoesNotContain(diagnostic, result.Output + result.Error);
            Assert.DoesNotContain("Exception", result.Output + result.Error);
        }
        finally { await db.Database.ExecuteSqlRawAsync("DROP TRIGGER dbo.RejectReset"); }
    }
}

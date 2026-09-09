// Acceptance Test: L2-050/AC1-2. Given an explicit target, inspection identifies SQL without mutation.
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class OperatorTargetCommandTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact]
    public async Task Given_a_named_target_when_inspected_then_the_actual_database_is_reported()
    {
        using var scope = factory.Services.CreateScope();
        var connection = new SqlConnectionStringBuilder(scope.ServiceProvider.GetRequiredService<EventDbContext>().Database.GetConnectionString());
        var config = Path.GetTempFileName();
        try
        {
            var configured = await ProvisioningProcess.Execute(factory, ["target", "configure", "--config", config,
                "--target", "test", "--server", connection.DataSource, "--database", connection.InitialCatalog,
                "--environment", "test", "--connection-env", "ConnectionStrings__EventDatabase"]);
            Assert.Equal(0, configured.ExitCode);
            var inspected = await ProvisioningProcess.Execute(factory, ["target", "inspect", "--config", config, "--target", "test", "--json"]);
            Assert.Equal(0, inspected.ExitCode);
            var result = JsonDocument.Parse(inspected.Output).RootElement;
            Assert.Equal(connection.InitialCatalog, result.GetProperty("target").GetProperty("database").GetString());
            Assert.False(string.IsNullOrEmpty(result.GetProperty("principal").GetProperty("key").GetString()));
            var missing = await ProvisioningProcess.Execute(factory, ["target", "inspect", "--config", config, "--target", "absent"]);
            Assert.Equal(2, missing.ExitCode);
        }
        finally { File.Delete(config); }
    }
}

using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using FaithTechTorontoAiBuildEvent.Provisioning;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class OperatorCliFixture : IDisposable
{
    public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), $"operator-{Guid.NewGuid():N}");
    public string Config => Path.Combine(DirectoryPath, "targets.json");
    public string Seed => Path.Combine(DirectoryPath, "event seed.json");
    public string[] Options => ["--config", Config, "--target", "test", "--json"];
    public OperatorCliFixture(EventApiFactory factory)
    {
        Directory.CreateDirectory(DirectoryPath);
        using var scope = factory.Services.CreateScope();
        var sql = new SqlConnectionStringBuilder(scope.ServiceProvider.GetRequiredService<EventDbContext>().Database.GetConnectionString());
        new OperatorTargetProfileStore(Config).Save("test", new(sql.DataSource, sql.InitialCatalog, "test", "ConnectionStrings__EventDatabase"));
        File.WriteAllBytes(Seed, JsonSerializer.SerializeToUtf8Bytes(new { @event = new { title = "Synthetic CLI event" }, schedule = September9Reference.Create() }, EventSeedReader.Json));
    }
    public void Dispose() => Directory.Delete(DirectoryPath, true);
}

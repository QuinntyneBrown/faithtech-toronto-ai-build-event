// Acceptance Test: L2-055/057/058. Rejected or failed imports preserve data and cannot duplicate it.
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventImportFailureTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact]
    public async Task Given_a_changed_file_when_apply_is_attempted_then_the_preview_is_rejected_without_mutation()
    {
        using var files = new OperatorCliFixture(factory);
        var preview = await Preview(files);
        await File.AppendAllTextAsync(files.Seed, " ");
        var result = await Apply(files, preview);
        Assert.Equal(4, result.ExitCode);
        using var scope = factory.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<EventDbContext>().Events.AnyAsync(x => x.Id == preview.GetProperty("result").GetProperty("import").GetProperty("eventId").GetGuid()));
    }

    [Fact]
    public async Task Given_a_stage_write_failure_when_import_runs_then_no_event_or_receipt_commits()
    {
        using var files = new OperatorCliFixture(factory);
        var preview = await Preview(files);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER RejectSeedStages ON dbo.Stages AFTER INSERT AS BEGIN THROW 51000, 'private-seed-diagnostic', 1; END");
        try {
            var result = await Apply(files, preview);
            Assert.Equal(1, result.ExitCode);
            Assert.DoesNotContain("private-seed-diagnostic", result.Output + result.Error);
            var operation = preview.GetProperty("operationId").GetGuid();
            Assert.False(await db.OperationReceipts.IgnoreQueryFilters().AnyAsync(x => x.OperationId == operation));
            var id = preview.GetProperty("result").GetProperty("import").GetProperty("eventId").GetGuid();
            Assert.False(await db.Events.AnyAsync(x => x.Id == id));
        } finally { await db.Database.ExecuteSqlRawAsync("DROP TRIGGER dbo.RejectSeedStages"); }
        Assert.Equal(0, (await Apply(files, preview)).ExitCode);
    }

    private async Task<JsonElement> Preview(OperatorCliFixture files)
    {
        var result = await ProvisioningProcess.Execute(factory, ["events", "import", "--create", "--file", files.Seed, "--preview", .. files.Options]);
        Assert.True(result.ExitCode == 0, result.Output + result.Error);
        return JsonDocument.Parse(result.Output).RootElement;
    }
    private Task<ProvisioningResult> Apply(OperatorCliFixture files, JsonElement preview)
    {
        var id = preview.GetProperty("previewId").GetGuid().ToString();
        return ProvisioningProcess.Execute(factory, ["operations", "apply", id, "--approve", id, .. files.Options]);
    }
}

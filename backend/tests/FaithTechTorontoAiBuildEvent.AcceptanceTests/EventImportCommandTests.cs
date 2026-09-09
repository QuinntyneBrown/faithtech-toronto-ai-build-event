// Acceptance Test: L2-054/057/058. The installed CLI requires a bound preview and supports reconciliation.
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventImportCommandTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact]
    public async Task Given_a_migration_preview_when_approved_then_schema_history_is_rechecked_and_no_pending_work_is_repeated()
    {
        using var files = new OperatorCliFixture(factory);
        var result = await ProvisioningProcess.Execute(factory, ["migrate", "--preview", .. files.Options]);
        Assert.True(result.ExitCode == 0, result.Output + result.Error);
        var id = JsonDocument.Parse(result.Output).RootElement.GetProperty("previewId").GetGuid().ToString();
        var applied = await ProvisioningProcess.Execute(factory, ["operations", "apply", id, "--approve", id, .. files.Options]);
        Assert.True(applied.ExitCode == 0, applied.Output + applied.Error);
        Assert.Equal("unchanged", JsonDocument.Parse(applied.Output).RootElement.GetProperty("outcome").GetString());
    }

    [Fact]
    public async Task Given_a_seed_when_previewed_then_no_event_is_written()
    {
        using var files = new OperatorCliFixture(factory);
        var preview = await ProvisioningProcess.Execute(factory, ["events", "import", "--create", "--file", files.Seed, "--preview", .. files.Options]);
        Assert.True(preview.ExitCode == 0, preview.Output + preview.Error);
        var result = JsonDocument.Parse(preview.Output).RootElement;
        Assert.Equal("previewed", result.GetProperty("outcome").GetString());
        var id = result.GetProperty("result").GetProperty("import").GetProperty("eventId").GetGuid();
        using var scope = factory.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<EventDbContext>().Events.AnyAsync(x => x.Id == id));
    }

    [Fact]
    public async Task Given_a_reviewed_seed_when_approved_and_retried_then_one_event_is_imported_and_reconciled()
    {
        using var files = new OperatorCliFixture(factory);
        var preview = await ProvisioningProcess.Execute(factory, ["events", "import", "--create", "--file", files.Seed, "--preview", .. files.Options]);
        Assert.True(preview.ExitCode == 0, preview.Output + preview.Error);
        var review = JsonDocument.Parse(preview.Output).RootElement;
        var previewId = review.GetProperty("previewId").GetGuid().ToString();
        var denied = await ProvisioningProcess.Execute(factory, ["operations", "apply", previewId, .. files.Options]);
        Assert.NotEqual(0, denied.ExitCode);
        var arguments = new[] { "operations", "apply", previewId, "--approve", previewId }.Concat(files.Options).ToArray();
        var applied = await ProvisioningProcess.Execute(factory, arguments);
        Assert.True(applied.ExitCode == 0, applied.Output + applied.Error);
        Assert.Equal(0, (await ProvisioningProcess.Execute(factory, arguments)).ExitCode);
        var reconciled = await ProvisioningProcess.Execute(factory, ["operations", "reconcile", review.GetProperty("operationId").GetGuid().ToString(), .. files.Options]);
        Assert.Equal(0, reconciled.ExitCode);
        Assert.Equal("reconciled", JsonDocument.Parse(reconciled.Output).RootElement.GetProperty("outcome").GetString());
    }
}

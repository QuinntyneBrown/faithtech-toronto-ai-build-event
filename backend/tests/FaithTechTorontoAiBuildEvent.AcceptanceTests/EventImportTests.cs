// Acceptance Test: L2-054/055/058. Import is atomic and retries identify one saved event.
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventImportTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact]
    public async Task Given_a_seed_when_imported_and_retried_then_one_draft_and_its_schedule_are_visible_through_the_api()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        await db.Database.OpenConnectionAsync();
        var principal = await SqlOperatorConnection.Identify((SqlConnection)db.Database.GetDbConnection(), "test", 60, default);
        var store = new SqlEventImportStore(db, principal);
        var input = EventSeedReader.Read(JsonSerializer.SerializeToUtf8Bytes(new { @event = new { title = "Synthetic September event", useLiturgy = false }, schedule = September9Reference.Create() }, EventSeedReader.Json));
        var review = await store.Review(Guid.NewGuid(), true, null, input, default);
        Assert.False(await db.Events.AnyAsync(x => x.Id == review.EventId));
        var operation = Guid.NewGuid();
        var saved = await store.Apply(operation, review, default);
        Assert.Equal(saved, await store.Apply(operation, review, default));
        using var browser = await factory.AdministratorBrowser();
        var detail = await browser.GetFromJsonAsync<JsonElement>($"/api/admin/events/{saved.EventId}");
        Assert.False(detail.GetProperty("published").GetBoolean());
        Assert.Equal("Synthetic September event", detail.GetProperty("title").GetString());
        Assert.Equal(21, await db.Stages.CountAsync(x => x.EventId == saved.EventId));
        Assert.Equal(1, await db.OperationReceipts.IgnoreQueryFilters().CountAsync(x => x.OperationId == operation));
    }
}

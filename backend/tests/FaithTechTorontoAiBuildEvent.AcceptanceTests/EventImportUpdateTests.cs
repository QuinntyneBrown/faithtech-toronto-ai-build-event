// Acceptance Test: L2-055/058. Updates retain history and reject stale or cross-event writes.
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventImportUpdateTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact]
    public async Task Given_an_existing_event_when_updated_then_history_remains_and_stale_writers_are_rejected()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        await db.Database.OpenConnectionAsync();
        var principal = await SqlOperatorConnection.Identify((SqlConnection)db.Database.GetDbConnection(), "test", 60, default);
        var store = new SqlEventImportStore(db, principal);
        var seed = EventSeedReader.Read(JsonSerializer.SerializeToUtf8Bytes(new { @event = new { title = "Original", address = "Retain" }, schedule = September9Reference.Create() }, EventSeedReader.Json));
        var create = await store.Review(Guid.NewGuid(), true, null, seed, default);
        var original = await store.Apply(Guid.NewGuid(), create, default);
        var registration = new Domain.Roster.Registration { EventId = original.EventId, DisplayName = "Synthetic participant", CodeDigest = "preserve-credential" };
        db.Registrations.Add(registration); await db.SaveChangesAsync();
        var patch = EventSeedReader.Read("{\"event\":{\"title\":\"Updated\",\"address\":null}}"u8.ToArray());
        var update = await store.Review(original.EventId, false, original.Version, patch, default);
        var operation = Guid.NewGuid();
        var changed = await store.Apply(operation, update, default);
        Assert.NotEqual(original.Version, changed.Version);
        var saved = await db.Events.AsNoTracking().SingleAsync(x => x.Id == original.EventId);
        Assert.Equal("Updated", saved.Title); Assert.Null(saved.Address);
        Assert.Equal("preserve-credential", (await db.Registrations.AsNoTracking().SingleAsync(x => x.Id == registration.Id)).CodeDigest);
        Assert.Equal(21, await db.Stages.CountAsync(x => x.EventId == original.EventId));
        await Assert.ThrowsAsync<StaleVersionException>(() => store.Apply(Guid.NewGuid(), update, default));
        Assert.Equal(changed, await store.Apply(operation, update, default));
        await Assert.ThrowsAsync<OperationConflictException>(() => store.Apply(operation, create, default));
        await Assert.ThrowsAsync<InputValidationException>(() => store.Review(Guid.NewGuid(), true, null, seed, default));
    }

    [Fact]
    public async Task Given_concurrent_retries_when_the_same_import_runs_then_both_observe_one_commit()
    {
        var id = Guid.NewGuid(); var operation = Guid.NewGuid();
        var seed = EventSeedReader.Read(JsonSerializer.SerializeToUtf8Bytes(new { @event = new { title = "Concurrent" }, schedule = September9Reference.Create() }, EventSeedReader.Json));
        async Task<EventImportResult> Run()
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventDbContext>(); await db.Database.OpenConnectionAsync();
            var principal = await SqlOperatorConnection.Identify((SqlConnection)db.Database.GetDbConnection(), "test", 60, default);
            var store = new SqlEventImportStore(db, principal);
            var after = EventSeedMerge.Merge(seed, new(null, null, null, null, null, null, null, null), new(null, null, null, [], null, null), true);
            return await store.Apply(operation, new(id, true, null, seed, null, after, false), default);
        }
        var results = await Task.WhenAll(Run(), Run()); Assert.Equal(results[0], results[1]);
    }
}

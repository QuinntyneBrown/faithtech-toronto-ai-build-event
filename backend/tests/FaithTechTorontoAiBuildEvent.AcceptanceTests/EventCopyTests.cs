using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventCopyTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-047/AC2")]
    public async Task Given_a_populated_event_with_shifted_intervals_when_copied_then_the_new_draft_is_a_shifted_unpublished_liturgy_off_clone()
    {
        using var client = await factory.AdministratorBrowser();
        var source = await Create(client);
        source = await SaveEvent(client, source, useLiturgy: true, directionsUrl: "https://example.org/directions");
        var schedule = await SaveSchedule(client, source);

        using var copyResponse = await Copy(client, source.GetProperty("id").GetGuid(), Time("2026-09-16T17:00"));
        Assert.Equal(HttpStatusCode.Created, copyResponse.StatusCode);
        var copy = await copyResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.NotEqual(source.GetProperty("id").GetGuid(), copy.GetProperty("id").GetGuid());
        Assert.False(copy.GetProperty("published").GetBoolean());
        Assert.False(copy.GetProperty("useLiturgy").GetBoolean());
        Assert.Equal("https://example.org/directions", copy.GetProperty("directionsUrl").GetString());
        Assert.Equal("America/Toronto", copy.GetProperty("timezone").GetString());
        Assert.Equal("2026-09-16T17:00:00", copy.GetProperty("start").GetProperty("local").GetString());
        Assert.Equal("2026-09-16T21:00:00", copy.GetProperty("end").GetProperty("local").GetString());

        var copySchedule = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{copy.GetProperty("id").GetGuid()}/schedule");
        var stages = copySchedule.GetProperty("stages");
        Assert.Equal(2, stages.GetArrayLength());
        var arrival = stages.EnumerateArray().Single(x => x.GetProperty("name").GetString() == "Arrival");
        Assert.Equal("2026-09-16T17:00:00", arrival.GetProperty("start").GetProperty("local").GetString());
        Assert.Equal("2026-09-16T18:00:00", arrival.GetProperty("end").GetProperty("local").GetString());
        Assert.Equal(-240, arrival.GetProperty("start").GetProperty("offsetMinutes").GetInt32());
        var sourceStageIds = schedule.GetProperty("stages").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToHashSet();
        Assert.DoesNotContain(arrival.GetProperty("id").GetGuid(), sourceStageIds);
        Assert.Equal("2026-09-16T18:00:00", copySchedule.GetProperty("selection").GetProperty("start").GetProperty("local").GetString());
        Assert.Equal("2026-09-16T19:00:00", copySchedule.GetProperty("presentation").GetProperty("start").GetProperty("local").GetString());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        Assert.Equal(0, await db.Registrations.CountAsync(x => x.EventId == copy.GetProperty("id").GetGuid()));
    }

    [Fact, Trait("Requirement", "L2-047/AC3")]
    public async Task Given_a_copied_event_when_the_original_is_later_saved_then_the_copy_is_unaffected_and_credentials_are_not_shared()
    {
        using var client = await factory.AdministratorBrowser();
        var source = await Create(client);
        source = await SaveEvent(client, source, useLiturgy: true, directionsUrl: "https://example.org/directions");
        await SaveSchedule(client, source);

        using var copyResponse = await Copy(client, source.GetProperty("id").GetGuid(), Time("2026-09-16T17:00"));
        var copy = await copyResponse.Content.ReadFromJsonAsync<JsonElement>();

        var reread = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{source.GetProperty("id").GetGuid()}");
        _ = await SaveEvent(client, reread, useLiturgy: true, directionsUrl: "https://example.org/directions", title: "Renamed after copy");

        var copyAfter = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{copy.GetProperty("id").GetGuid()}");
        Assert.NotEqual("Renamed after copy", copyAfter.GetProperty("title").GetString());

        using var addToSource = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{source.GetProperty("id").GetGuid()}/roster") { Content = JsonContent.Create(new { displayName = "Alex" }) };
        addToSource.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.SendAsync(addToSource)).EnsureSuccessStatusCode();
        var copyRoster = await client.GetFromJsonAsync<JsonElement[]>($"/api/admin/events/{copy.GetProperty("id").GetGuid()}/roster");
        Assert.Empty(copyRoster!);
    }

    [Fact, Trait("Requirement", "L2-047")]
    public async Task Given_dated_content_and_an_unresolved_source_start_when_copied_then_a_configuration_error_is_returned()
    {
        using var client = await factory.AdministratorBrowser();
        var source = await Create(client);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{source.GetProperty("id").GetGuid()}")
        {
            Content = JsonContent.Create(new { timezone = "America/Toronto", start = (object?)null, end = Time("2026-09-09T21:00") })
        };
        request.Headers.Add("If-Match", $"\"{source.GetProperty("version").GetString()}\"");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        (await client.SendAsync(request)).EnsureSuccessStatusCode();

        using var copyResponse = await Copy(client, source.GetProperty("id").GetGuid(), Time("2026-09-16T17:00"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, copyResponse.StatusCode);
        var problem = await copyResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("start", out _));
    }

    private static object Time(string local) => new { local, offsetMinutes = -240 };

    private static object Stage(string name, string start, string end) => new {
        id = Guid.NewGuid(), name, phase = "Develop", screenType = "information", content = "Plain text guidance",
        resourceUrl = "https://example.org/guide", start = Time(start), end = Time(end) };

    private static async Task<JsonElement> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Copy source" }); response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<JsonElement> SaveEvent(HttpClient client, JsonElement current, bool useLiturgy, string directionsUrl, string title = "Copy source")
    {
        var timezone = current.TryGetProperty("timezone", out var tz) && tz.ValueKind == JsonValueKind.String ? tz.GetString() : null;
        var start = current.TryGetProperty("start", out var s) && s.ValueKind == JsonValueKind.Object ? (object?)s : null;
        var end = current.TryGetProperty("end", out var e) && e.ValueKind == JsonValueKind.Object ? (object?)e : null;
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{current.GetProperty("id").GetGuid()}")
        {
            Content = JsonContent.Create(new { title, venueName = "Stone Church", address = "123 Main St", directionsUrl, useLiturgy, timezone, start, end })
        };
        request.Headers.Add("If-Match", $"\"{current.GetProperty("version").GetString()}\"");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.SendAsync(request); response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<JsonElement> SaveSchedule(HttpClient client, JsonElement current)
    {
        var input = new { timezone = "America/Toronto", start = Time("2026-09-09T17:00"), end = Time("2026-09-09T21:00"),
            stages = new[] { Stage("Arrival", "2026-09-09T17:00", "2026-09-09T18:00"), Stage("Build", "2026-09-09T18:00", "2026-09-09T20:00") },
            selection = new { start = Time("2026-09-09T18:00"), end = Time("2026-09-09T19:00") },
            presentation = new { start = Time("2026-09-09T19:00"), end = Time("2026-09-09T20:00") } };
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{current.GetProperty("id").GetGuid()}/schedule") { Content = JsonContent.Create(input) };
        request.Headers.Add("If-Match", $"\"{current.GetProperty("version").GetString()}\"");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.SendAsync(request); response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<HttpResponseMessage> Copy(HttpClient client, Guid eventId, object start)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{eventId}/copy") { Content = JsonContent.Create(new { start }) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }
}

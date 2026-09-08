using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventTimeTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-006/AC4;L2-006/AC6;L2-001/AC6")]
    [InlineData("America/Toronto", "2026-11-01T01:30:00", null, "2026-11-01T03:00:00", 422)]
    [InlineData("America/Toronto", "2026-11-01T01:30:00", -240, "2026-11-01T03:00:00", 200)]
    [InlineData("America/Toronto", "2026-11-01T01:30:00", -300, "2026-11-01T03:00:00", 200)]
    [InlineData("America/Toronto", "2026-03-08T02:30:00", -300, "2026-03-08T04:00:00", 422)]
    [InlineData("America/Toronto", "2026-09-09T23:00:00", null, "2026-09-10T01:00:00", 200)]
    [InlineData("America/Toronto", "2026-09-09T23:00:00", 0, "2026-09-10T01:00:00", 422)]
    [InlineData("America/Toronto", "2026-09-09T23:00:00", null, "2026-09-09T21:00:00", 422)]
    [InlineData("Missing/Zone", "2026-09-09T17:00:00", null, "2026-09-09T21:00:00", 422)]
    public async Task Given_dated_local_times_when_saved_then_only_unambiguous_positive_intervals_are_retained(
        string timezone, string localStart, int? offsetMinutes, string localEnd, int expectedStatus)
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "Time validation" });
        var original = await created.Content.ReadFromJsonAsync<JsonElement>();
        var path = $"/api/admin/events/{original.GetProperty("id").GetGuid()}";
        client.DefaultRequestHeaders.Add("If-Match", $"\"{original.GetProperty("version").GetString()}\"");
        var response = await client.PutAsJsonAsync(path, new { title = "Time validation", timezone,
            start = new { local = localStart, offsetMinutes }, end = new { local = localEnd } });
        Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);
        var reloaded = await client.GetFromJsonAsync<JsonElement>(path);
        if (expectedStatus == 200)
        {
            Assert.Equal(timezone, reloaded.GetProperty("timezone").GetString());
            var start = reloaded.GetProperty("startsAtUtc").GetDateTimeOffset();
            var end = reloaded.GetProperty("endsAtUtc").GetDateTimeOffset();
            Assert.True(end > start);
            Assert.Equal(TimeSpan.Zero, start.Offset);
            Assert.Equal(DateTime.Parse(localStart), reloaded.GetProperty("start").GetProperty("local").GetDateTime());
            if (offsetMinutes.HasValue) Assert.Equal(offsetMinutes, reloaded.GetProperty("start").GetProperty("offsetMinutes").GetInt32());
        }
        else Assert.Equal(original.GetProperty("version").GetString(), reloaded.GetProperty("version").GetString());
    }
}

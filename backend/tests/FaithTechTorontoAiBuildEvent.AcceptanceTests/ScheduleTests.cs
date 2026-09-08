using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ScheduleTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-006/AC1;L2-006/AC6")]
    [InlineData("2026-09-10T00:00", HttpStatusCode.OK)]
    [InlineData("2026-09-09T23:59", HttpStatusCode.UnprocessableEntity)]
    public async Task Given_overnight_stages_when_saved_then_adjacency_is_accepted_and_overlap_preserves_the_event(string secondStart, HttpStatusCode expected)
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Create(client);
        var input = new { timezone = "America/Toronto", start = Time("2026-09-09T23:00"), end = Time("2026-09-10T01:00"),
            stages = new[] { Stage("Arrival", "2026-09-09T23:00", "2026-09-10T00:00"), Stage("Build", secondStart, "2026-09-10T01:00") },
            selection = new { start = Time("2026-09-09T23:30"), end = Time("2026-09-10T00:30") }, presentation = (object?)null };
        using var response = await Save(client, original, input);
        Assert.Equal(expected, response.StatusCode);
        var id = original.GetProperty("id").GetGuid();
        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{id}");
        if (expected == HttpStatusCode.OK) {
            var saved = await response.Content.ReadFromJsonAsync<JsonElement>();
            var reread = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{id}/schedule");
            Assert.Equal(saved.ToString(), reread.ToString());
            Assert.Equal(2, reread.GetProperty("stages").GetArrayLength());
            Assert.Equal("Arrival", reread.GetProperty("stages")[0].GetProperty("name").GetString());
            Assert.Equal("2026-09-10T03:00:00+00:00", detail.GetProperty("startsAtUtc").GetString());
            Assert.Equal(detail.GetProperty("version").GetString(), reread.GetProperty("version").GetString());
        } else {
            Assert.Equal(original.GetProperty("version").GetString(), detail.GetProperty("version").GetString());
            Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors").TryGetProperty("stages[1].start", out _));
        }
    }

    private static object Time(string local) => new { local, offsetMinutes = -240 };
    private static object Stage(string name, string start, string end) => new {
        id = Guid.NewGuid(), name, phase = "Develop", screenType = "information", content = "Plain text guidance",
        resourceUrl = "https://example.org/guide", start = Time(start), end = Time(end) };
    private static async Task<JsonElement> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Overnight build" });
        response.EnsureSuccessStatusCode(); client.DefaultRequestHeaders.Remove("Idempotency-Key");
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
    private static async Task<HttpResponseMessage> Save(HttpClient client, JsonElement current, object input, string? operationId = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{current.GetProperty("id").GetGuid()}/schedule") { Content = JsonContent.Create(input) };
        request.Headers.Add("If-Match", $"\"{current.GetProperty("version").GetString()}\"");
        request.Headers.Add("Idempotency-Key", operationId ?? Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }
}

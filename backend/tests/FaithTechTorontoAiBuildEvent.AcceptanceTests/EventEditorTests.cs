using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventEditorTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-001/AC1;L2-001/AC6;L2-040/AC1")]
    public async Task Given_incomplete_configuration_when_saved_then_content_is_normalized_and_invalid_directions_preserve_the_draft()
    {
        using var client = await factory.AdministratorBrowser();
        var first = await Create(client, "Original");
        var path = $"/api/admin/events/{first.GetProperty("id").GetGuid()}";
        using var request = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(new {
            title = "  Updated  ", venueName = "  Toronto venue  ", address = "  One street  ", latitude = 43.65, longitude = -79.38,
            waitingContent = "  Welcome\r\n<script>plain text</script>  ", closingContent = " Thank you ", directionsUrl = " https://example.org/directions " }) };
        request.Headers.Add("If-Match", $"\"{first.GetProperty("version").GetString()}\"");
        var saved = await client.SendAsync(request);
        saved.EnsureSuccessStatusCode();
        var detail = await client.GetFromJsonAsync<JsonElement>(path);
        Assert.Equal("Toronto venue", detail.GetProperty("venueName").GetString());
        Assert.Equal("One street", detail.GetProperty("address").GetString());
        Assert.Equal(43.65, detail.GetProperty("latitude").GetDouble());
        Assert.Equal(-79.38, detail.GetProperty("longitude").GetDouble());
        Assert.Equal("Welcome\n<script>plain text</script>", detail.GetProperty("waitingContent").GetString());
        Assert.Equal("Thank you", detail.GetProperty("closingContent").GetString());
        Assert.Equal("https://example.org/directions", detail.GetProperty("directionsUrl").GetString());
        foreach (var url in new[] { "http://example.org", "/relative", "https://user:password@example.org" })
        {
            using var invalid = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(new { directionsUrl = url }) };
            invalid.Headers.Add("If-Match", $"\"{detail.GetProperty("version").GetString()}\"");
            invalid.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
            var rejected = await client.SendAsync(invalid);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, rejected.StatusCode);
            var problem = await rejected.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(problem.GetProperty("errors").TryGetProperty("directionsUrl", out _));
        }
        Assert.Equal(detail.ToString(), (await client.GetFromJsonAsync<JsonElement>(path)).ToString());
    }

    [Fact, Trait("Requirement", "L2-001/AC1;L2-044/AC4;L2-044/AC6")]
    public async Task Given_two_editors_when_one_saves_then_stale_writes_fail_and_committed_retries_return_the_original_result()
    {
        using var client = await factory.AdministratorBrowser();
        var first = await Create(client, "Original");
        var other = await Create(client, "Other event");
        var path = $"/api/admin/events/{first.GetProperty("id").GetGuid()}";
        var operation = Guid.NewGuid().ToString();
        using var saved = await Save(client, path, first, operation, "  Updated  ");
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var result = await saved.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated", result.GetProperty("title").GetString());
        Assert.NotEqual(first.GetProperty("version").GetString(), result.GetProperty("version").GetString());
        using var stale = await Save(client, path, first, Guid.NewGuid().ToString(), "Stale edit");
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var problem = await stale.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("stale-version", problem.GetProperty("code").GetString());
        Assert.Equal("Updated", problem.GetProperty("current").GetProperty("title").GetString());
        using var later = await Save(client, path, result, Guid.NewGuid().ToString(), "Later edit");
        later.EnsureSuccessStatusCode();
        using var retry = await Save(client, path, first, operation, "Updated");
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal(result.ToString(), (await retry.Content.ReadFromJsonAsync<JsonElement>()).ToString());
        using var changed = await Save(client, path, first, operation, "Changed intent");
        Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
        Assert.Equal("Later edit", (await client.GetFromJsonAsync<JsonElement>(path)).GetProperty("title").GetString());
        var unchanged = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{other.GetProperty("id").GetGuid()}");
        Assert.Equal("Other event", unchanged.GetProperty("title").GetString());
    }

    private static async Task<JsonElement> Create(HttpClient client, string title)
    {
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var response = await client.PostAsJsonAsync("/api/admin/events", new { title });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static Task<HttpResponseMessage> Save(HttpClient client, string path, JsonElement original, string operation, string title)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(new { title }) };
        request.Headers.Add("Idempotency-Key", operation);
        request.Headers.Add("If-Match", $"\"{original.GetProperty("version").GetString()}\"");
        return client.SendAsync(request);
    }

    [Fact, Trait("Requirement", "L2-047/AC1;L2-039")]
    public async Task Given_a_saved_draft_when_opened_then_its_values_and_version_are_available_only_to_administrators()
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "Toronto" });
        var draft = await created.Content.ReadFromJsonAsync<JsonElement>();
        var path = $"/api/admin/events/{draft.GetProperty("id").GetGuid()}";
        var detail = await client.GetFromJsonAsync<JsonElement>(path);
        Assert.Equal("Toronto", detail.GetProperty("title").GetString());
        Assert.Equal(draft.GetProperty("version").GetString(), detail.GetProperty("version").GetString());
        Assert.False(detail.GetProperty("published").GetBoolean());
        using var anonymous = factory.Browser();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/admin/events/{Guid.NewGuid()}")).StatusCode);
    }
}

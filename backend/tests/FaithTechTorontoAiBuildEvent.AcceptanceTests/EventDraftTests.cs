using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventDraftTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-001/AC2;L2-044/AC2")]
    public async Task Given_missing_publication_fields_when_creating_and_retrying_a_draft_then_one_draft_is_retained()
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "  Toronto  " });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var first = await created.Content.ReadFromJsonAsync<JsonElement>();
        var retry = await client.PostAsJsonAsync("/api/admin/events", new { title = "Toronto" });
        var second = await retry.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(first.GetProperty("id").GetGuid(), second.GetProperty("id").GetGuid());
        Assert.False(first.GetProperty("published").GetBoolean());
        Assert.False(first.GetProperty("useLiturgy").GetBoolean());
        Assert.Equal("Toronto", first.GetProperty("title").GetString());
        var list = await client.GetFromJsonAsync<JsonElement>("/api/admin/events");
        Assert.Contains(list.EnumerateArray(), x => x.GetProperty("id").GetGuid() == first.GetProperty("id").GetGuid());
        var changed = await client.PostAsJsonAsync("/api/admin/events", new { title = "Changed intent" });
        Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
    }

    [Fact, Trait("Requirement", "L2-040/AC1")]
    public async Task Given_an_oversized_title_when_creating_a_draft_then_the_field_is_rejected()
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var response = await client.PostAsJsonAsync("/api/admin/events", new { title = new string('a', 201) });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("title", out _));
    }
}

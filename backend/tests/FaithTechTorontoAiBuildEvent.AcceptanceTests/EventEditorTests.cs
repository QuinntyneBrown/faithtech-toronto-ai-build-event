using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventEditorTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
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

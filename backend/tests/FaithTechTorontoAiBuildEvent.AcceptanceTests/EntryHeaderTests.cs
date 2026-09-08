using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EntryHeaderTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-046/AC1")]
    public async Task Given_an_unknown_event_when_the_entry_header_is_requested_then_it_is_unavailable()
    {
        using var anonymous = factory.Browser();
        using var response = await anonymous.GetAsync($"/api/events/{Guid.NewGuid()}/entry");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Trait("Requirement", "L2-046/AC1")]
    public async Task Given_a_draft_event_when_the_entry_header_is_requested_then_the_same_unavailable_response_is_returned_as_for_an_unknown_event()
    {
        using var admin = await factory.AdministratorBrowser();
        var item = await Create(admin);
        using var anonymous = factory.Browser();

        using var draftResponse = await anonymous.GetAsync($"/api/events/{item.GetProperty("id").GetGuid()}/entry");
        using var unknownResponse = await anonymous.GetAsync($"/api/events/{Guid.NewGuid()}/entry");
        Assert.Equal(HttpStatusCode.NotFound, draftResponse.StatusCode);
        Assert.Equal(unknownResponse.StatusCode, draftResponse.StatusCode);
        var draftBody = await draftResponse.Content.ReadFromJsonAsync<JsonElement>();
        var unknownBody = await unknownResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(unknownBody.GetProperty("status").GetInt32(), draftBody.GetProperty("status").GetInt32());
        Assert.Equal(unknownBody.GetProperty("title").GetString(), draftBody.GetProperty("title").GetString());
        Assert.DoesNotContain("eventId", draftBody.EnumerateObject().Select(x => x.Name));
    }

    [Fact, Trait("Requirement", "L2-046/AC1")]
    public async Task Given_a_published_event_when_the_entry_header_is_requested_then_only_the_event_id_and_title_are_returned()
    {
        using var admin = await factory.AdministratorBrowser();
        var item = await Create(admin);
        var eventId = item.GetProperty("id").GetGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            var entity = await db.Events.SingleAsync(x => x.Id == eventId);
            entity.Published = true; entity.VenueName = "Stone Church"; entity.Address = "123 Main St";
            await db.SaveChangesAsync();
        }

        using var anonymous = factory.Browser();
        using var response = await anonymous.GetAsync($"/api/events/{eventId}/entry");
        response.EnsureSuccessStatusCode();
        var header = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(eventId, header.GetProperty("eventId").GetGuid());
        Assert.Equal("Entry header", header.GetProperty("title").GetString());
        var properties = header.EnumerateObject().Select(x => x.Name).ToArray();
        Assert.Equal(["eventId", "title"], properties.OrderBy(x => x, StringComparer.Ordinal));
    }

    private static async Task<JsonElement> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Entry header" }); response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}

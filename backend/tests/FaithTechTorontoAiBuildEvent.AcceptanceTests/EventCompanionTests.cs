using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventCompanionTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-026")]
    public async Task Given_a_new_event_when_an_administrator_toggles_the_companion_setting_then_it_defaults_off_and_changes_only_that_event()
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "Optional companion" });
        var summary = (await created.Content.ReadFromJsonAsync<EventSummary>())!;
        Assert.False(summary.UseLiturgy);
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        var path = $"/api/admin/events/{summary.Id}";
        var current = (await client.GetFromJsonAsync<EventDetail>(path))!;
        foreach (var enabled in new[] { true, false, true }) {
            using var request = new HttpRequestMessage(HttpMethod.Put, path) {
                Content = JsonContent.Create(new { title = current.Title, useLiturgy = enabled }) };
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
            request.Headers.Add("If-Match", $"\"{current.Version}\"");
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            current = (await response.Content.ReadFromJsonAsync<EventDetail>())!;
            Assert.Equal(enabled, current.UseLiturgy);
            Assert.Equal(enabled, (await client.GetFromJsonAsync<EventDetail>(path))!.UseLiturgy);
        }
        using var anonymous = factory.Browser();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PutAsJsonAsync(path, new { useLiturgy = false })).StatusCode);
        Assert.True((await client.GetFromJsonAsync<EventDetail>(path))!.UseLiturgy);
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var other = await client.PostAsJsonAsync("/api/admin/events", new { title = "Independent event" });
        Assert.False((await other.Content.ReadFromJsonAsync<EventSummary>())!.UseLiturgy);
    }
}

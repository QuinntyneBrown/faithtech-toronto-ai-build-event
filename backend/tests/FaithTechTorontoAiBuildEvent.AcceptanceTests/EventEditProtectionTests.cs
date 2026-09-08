using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventEditProtectionTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-038;L2-041;L2-044/AC4")]
    public async Task Given_a_draft_when_a_save_lacks_authorization_csrf_or_version_then_no_changes_are_committed()
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "Original" });
        var original = await created.Content.ReadFromJsonAsync<JsonElement>();
        var path = $"/api/admin/events/{original.GetProperty("id").GetGuid()}";
        using var anonymous = factory.Browser();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PutAsJsonAsync(path, new { title = "Changed" })).StatusCode);
        Assert.Equal((HttpStatusCode)428, (await client.PutAsJsonAsync(path, new { title = "Changed" })).StatusCode);
        client.DefaultRequestHeaders.Add("If-Match", $"\"{original.GetProperty("version").GetString()}\"");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(path, new { title = "Changed" })).StatusCode);
        var unchanged = await client.GetFromJsonAsync<JsonElement>(path);
        Assert.Equal("Original", unchanged.GetProperty("title").GetString());
        Assert.Equal(original.GetProperty("version").GetString(), unchanged.GetProperty("version").GetString());
    }

    [Fact, Trait("Requirement", "L2-044/AC4;L2-045/AC3")]
    public async Task Given_simultaneous_editors_when_they_save_one_version_then_exactly_one_update_and_audit_are_committed()
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "Original" });
        var original = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = original.GetProperty("id").GetGuid();
        var path = $"/api/admin/events/{id}";
        client.DefaultRequestHeaders.Remove("Idempotency-Key");
        client.DefaultRequestHeaders.Add("If-Match", $"\"{original.GetProperty("version").GetString()}\"");
        var requests = new[] { "First proposed title", "Second proposed title" }.Select(title => {
            var request = new HttpRequestMessage(HttpMethod.Put, path) { Content = JsonContent.Create(new { title }) };
            request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString()); return request;
        }).ToArray();
        var responses = await Task.WhenAll(requests.Select(request => client.SendAsync(request)));
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        var saved = await responses.Single(response => response.IsSuccessStatusCode).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(saved.ToString(), (await client.GetFromJsonAsync<JsonElement>(path)).ToString());
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var audit = await db.AuditRecords.Where(record => record.EventId == id && record.Action == "event-saved").SingleAsync();
        Assert.NotEqual(Guid.Empty, audit.ActorId);
        Assert.Equal("succeeded", audit.Outcome);
        Assert.Single(await db.OperationReceipts.Where(receipt => receipt.EventId == id).ToListAsync());
        foreach (var request in requests) request.Dispose();
        foreach (var response in responses) response.Dispose();
    }
}

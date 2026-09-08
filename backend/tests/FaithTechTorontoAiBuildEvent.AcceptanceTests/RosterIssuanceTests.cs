using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class RosterIssuanceTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-002/AC1;L2-041;L2-044;L2-045")]
    public async Task Given_duplicate_display_names_when_registered_then_identities_and_codes_differ_and_retries_do_not_reveal_or_issue_codes()
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "Roster acceptance" }); created.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); var item = (await created.Content.ReadFromJsonAsync<EventSummary>())!;
        var operation = Guid.NewGuid();
        async Task<HttpResponseMessage> Add(Guid key, string name = "  Alex  ") {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{item.Id}/roster") { Content = JsonContent.Create(new { displayName = name }) };
            request.Headers.Add("Idempotency-Key", key.ToString()); return await client.SendAsync(request);
        }
        using var first = await Add(operation); first.EnsureSuccessStatusCode();
        var issued = await first.Content.ReadFromJsonAsync<JsonElement>();
        var code = issued.GetProperty("code").GetString()!;
        Assert.True(Convert.FromHexString(code).Length >= 16);
        Assert.Contains("no-store", first.Headers.CacheControl!.ToString());
        Assert.Equal("Alex", issued.GetProperty("entry").GetProperty("displayName").GetString());
        using var second = await Add(Guid.NewGuid()); second.EnsureSuccessStatusCode();
        var other = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotEqual(code, other.GetProperty("code").GetString());
        Assert.NotEqual(issued.GetProperty("entry").GetProperty("id").GetGuid(), other.GetProperty("entry").GetProperty("id").GetGuid());
        using var retry = await Add(operation); retry.EnsureSuccessStatusCode();
        var completed = await retry.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Null, completed.GetProperty("code").ValueKind);
        Assert.True(completed.GetProperty("previouslyCompleted").GetBoolean());
        Assert.Equal(issued.GetProperty("entry").GetProperty("id").GetGuid(), completed.GetProperty("entry").GetProperty("id").GetGuid());
        var roster = await client.GetFromJsonAsync<JsonElement>($"/api/admin/events/{item.Id}/roster");
        Assert.Equal(2, roster.GetArrayLength()); Assert.DoesNotContain(code, roster.ToString());
        Assert.All(roster.EnumerateArray(), entry => { Assert.False(entry.TryGetProperty("code", out _)); Assert.True(entry.GetProperty("active").GetBoolean()); });
        using var changed = await Add(operation, "Different name"); Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
        using var scope = factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var registration = await db.Registrations.SingleAsync(x => x.Id == issued.GetProperty("entry").GetProperty("id").GetGuid());
        Assert.NotEqual(code, registration.CodeDigest); Assert.Equal(64, registration.CodeDigest.Length);
        Assert.DoesNotContain(code, JsonSerializer.Serialize(registration));
        var receipts = await db.OperationReceipts.Where(x => x.EventId == item.Id).Select(x => x.Result).ToListAsync();
        Assert.All(receipts, receipt => Assert.DoesNotContain(code, receipt));
        Assert.Equal(2, await db.AuditRecords.CountAsync(x => x.EventId == item.Id && x.Action == "registration-added"));
    }
}

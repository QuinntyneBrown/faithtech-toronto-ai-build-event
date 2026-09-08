using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantAbuseTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Requirement", "L2-042/AC1;L2-042/AC4")]
    public async Task Given_five_failures_when_retrying_a_participant_code_then_a_retry_delay_is_returned(bool existing)
    {
        var factory = new EventApiFactory();
        await factory.InitializeAsync();
        try
        {
            using var admin = await factory.AdministratorBrowser();
            var eventId = await CreatePublishedEvent(admin, factory);
            string code;
            if (existing)
            {
                var (_, issuedCode) = await AddParticipant(admin, eventId, "Alex");
                using var bind = factory.Browser();
                (await Authenticate(bind, eventId, "alex@example.com", issuedCode)).EnsureSuccessStatusCode();
                code = issuedCode;
            }
            else code = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

            using var client = factory.Browser();
            for (var attempt = 0; attempt < 5; attempt++)
                Assert.Equal(HttpStatusCode.Unauthorized, (await Authenticate(client, eventId, "someone-else@example.com", code)).StatusCode);
            var response = await Authenticate(client, eventId, "yet-another@example.com", $" {code.ToUpperInvariant()} ");
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
            Assert.True(response.Headers.RetryAfter?.Delta > TimeSpan.Zero);
            Assert.False(response.Headers.Contains("Set-Cookie"));
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            Assert.Equal(5, await db.AuthenticationFailures.CountAsync());
            await db.AuthenticationFailures.ExecuteUpdateAsync(set => set.SetProperty(x => x.FailedAtUtc, DateTimeOffset.UtcNow.AddMinutes(-6)));
            response = await Authenticate(client, eventId, existing ? "alex@example.com" : "someone-else@example.com", code);
            Assert.Equal(existing ? HttpStatusCode.OK : HttpStatusCode.Unauthorized, response.StatusCode);
        }
        finally { await ((IAsyncLifetime)factory).DisposeAsync(); }
    }

    [Fact, Trait("Requirement", "L2-042/AC1")]
    public async Task Given_ten_failures_for_distinct_codes_when_the_source_retries_then_it_is_throttled()
    {
        var factory = new EventApiFactory();
        await factory.InitializeAsync();
        try
        {
            using var admin = await factory.AdministratorBrowser();
            var eventId = await CreatePublishedEvent(admin, factory);
            using var client = factory.Browser();
            for (var index = 0; index < 10; index++)
                Assert.Equal(HttpStatusCode.Unauthorized, (await Authenticate(client, eventId, "someone@example.com", $"unknown-{index}")).StatusCode);
            Assert.Equal(HttpStatusCode.TooManyRequests, (await Authenticate(client, eventId, "someone@example.com", "different")).StatusCode);
        }
        finally { await ((IAsyncLifetime)factory).DisposeAsync(); }
    }

    [Fact, Trait("Requirement", "L2-042/AC1")]
    public async Task Given_simultaneous_failures_when_the_code_budget_is_reached_then_only_five_attempts_are_tested()
    {
        var factory = new EventApiFactory();
        await factory.InitializeAsync();
        try
        {
            using var admin = await factory.AdministratorBrowser();
            var eventId = await CreatePublishedEvent(admin, factory);
            using var client = factory.Browser();
            var token = await client.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/antiforgery");
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
            var responses = await Task.WhenAll(Enumerable.Range(0, 7).Select(_ =>
                client.PostAsJsonAsync($"/api/events/{eventId}/session", new { email = "someone@example.com", entryCode = "unknown-code" })));
            Assert.Equal(5, responses.Count(x => x.StatusCode == HttpStatusCode.Unauthorized));
            Assert.Equal(2, responses.Count(x => x.StatusCode == HttpStatusCode.TooManyRequests));
            foreach (var response in responses) response.Dispose();
        }
        finally { await ((IAsyncLifetime)factory).DisposeAsync(); }
    }

    private static async Task<Guid> CreatePublishedEvent(HttpClient admin, EventApiFactory factory)
    {
        admin.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await admin.PostAsJsonAsync("/api/admin/events", new { title = "Participant abuse" }); response.EnsureSuccessStatusCode();
        admin.DefaultRequestHeaders.Remove("Idempotency-Key");
        var item = await response.Content.ReadFromJsonAsync<JsonElement>();
        var eventId = item.GetProperty("id").GetGuid();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        (await db.Events.SingleAsync(x => x.Id == eventId)).Published = true;
        await db.SaveChangesAsync();
        return eventId;
    }

    private static async Task<(Guid RegistrationId, string Code)> AddParticipant(HttpClient admin, Guid eventId, string name)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{eventId}/roster") { Content = JsonContent.Create(new { displayName = name }) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await admin.SendAsync(request); response.EnsureSuccessStatusCode();
        var issued = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (issued.GetProperty("entry").GetProperty("id").GetGuid(), issued.GetProperty("code").GetString()!);
    }

    private static async Task<HttpResponseMessage> Authenticate(HttpClient client, Guid eventId, string email, string entryCode)
    {
        var token = await client.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/antiforgery");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.PostAsJsonAsync($"/api/events/{eventId}/session", new { email, entryCode });
    }
}

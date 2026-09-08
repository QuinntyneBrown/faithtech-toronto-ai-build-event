using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorAbuseTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Requirement", "L2-042/AC1;L2-042/AC4")]
    public async Task Given_five_failures_when_retrying_the_normalized_account_then_a_retry_delay_is_returned(bool existing)
    {
        var factory = new EventApiFactory();
        await factory.InitializeAsync();
        try
        {
            var password = $"Valid9!{Guid.NewGuid():N}";
            var username = existing ? await factory.ProvisionAdministrator(password) : "unknown";
            using var client = factory.Browser();
            var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
            for (var attempt = 0; attempt < 5; attempt++)
                Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/admin/session", new { username, password = "wrong" })).StatusCode);
            var response = await client.PostAsJsonAsync("/api/admin/session", new { username = $" {username.ToUpperInvariant()} ", password });
            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
            Assert.True(response.Headers.RetryAfter?.Delta > TimeSpan.Zero);
            Assert.False(response.Headers.Contains("Set-Cookie"));
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            Assert.Equal(5, await db.AuthenticationFailures.CountAsync());
            await db.AuthenticationFailures.ExecuteUpdateAsync(set => set.SetProperty(x => x.FailedAtUtc, DateTimeOffset.UtcNow.AddMinutes(-6)));
            response = await client.PostAsJsonAsync("/api/admin/session", new { username, password });
            Assert.Equal(existing ? HttpStatusCode.NoContent : HttpStatusCode.Unauthorized, response.StatusCode);
        }
        finally { await ((IAsyncLifetime)factory).DisposeAsync(); }
    }

    [Fact, Trait("Requirement", "L2-042/AC1")]
    public async Task Given_ten_failures_for_distinct_accounts_when_the_source_retries_then_it_is_throttled()
    {
        var factory = new EventApiFactory();
        await factory.InitializeAsync();
        try
        {
            using var client = factory.Browser();
            var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
            for (var index = 0; index < 10; index++)
                Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/admin/session", new { username = $"unknown-{index}", password = "wrong" })).StatusCode);
            Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/admin/session", new { username = "different", password = "wrong" })).StatusCode);
        }
        finally { await ((IAsyncLifetime)factory).DisposeAsync(); }
    }

    [Fact, Trait("Requirement", "L2-042/AC1")]
    public async Task Given_simultaneous_failures_when_the_account_budget_is_reached_then_only_five_credentials_are_tested()
    {
        var factory = new EventApiFactory();
        await factory.InitializeAsync();
        try
        {
            using var client = factory.Browser();
            var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
            var responses = await Task.WhenAll(Enumerable.Range(0, 7).Select(_ =>
                client.PostAsJsonAsync("/api/admin/session", new { username = "unknown", password = "wrong" })));
            Assert.Equal(5, responses.Count(x => x.StatusCode == HttpStatusCode.Unauthorized));
            Assert.Equal(2, responses.Count(x => x.StatusCode == HttpStatusCode.TooManyRequests));
            foreach (var response in responses) response.Dispose();
        }
        finally { await ((IAsyncLifetime)factory).DisposeAsync(); }
    }
}

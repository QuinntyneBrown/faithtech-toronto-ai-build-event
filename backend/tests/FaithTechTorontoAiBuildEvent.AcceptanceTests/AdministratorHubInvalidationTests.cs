// Acceptance Test
// Traces to: L2-038, L2-044
// Description: Credential rotation invalidates an already-connected private administrator stream within two seconds.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using FaithTechTorontoAiBuildEvent.Api.Hubs;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorHubInvalidationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdministratorHubInvalidationTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Credential_rotation_invalidates_connected_administrator_stream()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false
        });
        var signedIn = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        Assert.Equal(HttpStatusCode.OK, signedIn.StatusCode);
        var secret = signedIn.Headers.GetValues("Set-Cookie").Single()
            .Split(';', 2)[0]
            .Split('=', 2)[1];
        var invalidated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var connection = new HubConnectionBuilder()
            .WithUrl("https://localhost/api/admin/updates", options =>
            {
                options.Headers.Add("Cookie", $"faithtech-admin={secret}");
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            })
            .Build();
        connection.On("sessionInvalidated", () => invalidated.TrySetResult());
        await connection.StartAsync();
        var registry = factory.Services.GetRequiredService<IPrivateConnectionRegistry>();
        var connectedBy = DateTimeOffset.UtcNow.AddSeconds(1);
        while (registry.Snapshot().Count == 0 && DateTimeOffset.UtcNow < connectedBy) await Task.Delay(10);

        await factory.ProvisionAdministratorPasscodeAsync("0042");

        await invalidated.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }
}

// Acceptance Test
// Traces to: L2-020, L2-021
// Description: An administrator on the live Raffle stage draws one durable eligible winner.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class RaffleDrawTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public RaffleDrawTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Live_administrator_draws_only_eligible_participant_once()
    {
        using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt.OperationId, expectedVersion = "0", input = new { email = "raffle@example.com" } });

        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "1", fromScreen = "countdown", toScreen = "projects" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "2", fromScreen = "projects", toScreen = "teams" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "3", fromScreen = "teams", toScreen = "raffle" });

        var draw = await administrator.PostAsJsonAsync("/api/admin/raffle/draws", new { operationId = Guid.NewGuid(), expectedVersion = "4" });
        var result = await draw.Content.ReadFromJsonAsync<DrawResponse>();

        Assert.Equal(HttpStatusCode.OK, draw.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Participant 001", result.WinnerLabel);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record DrawResponse(string WinnerLabel);
}

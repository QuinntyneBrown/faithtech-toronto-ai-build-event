// Acceptance Test
// Traces to: L2-021
// Description: A second current-version draw cannot consume another participant while the saved first-draw effects window is active.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class RaffleActiveDrawTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public RaffleActiveDrawTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Current_version_second_draw_is_rejected_until_first_effects_finish()
    {
        await EnterAsync("first@example.com", "0");
        await EnterAsync("second@example.com", "1");
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "2", fromScreen = "countdown", toScreen = "projects" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "3", fromScreen = "projects", toScreen = "teams" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "4", fromScreen = "teams", toScreen = "raffle" });

        var first = await administrator.PostAsJsonAsync("/api/admin/raffle/draws", new { operationId = Guid.NewGuid(), expectedVersion = "5" });
        var second = await administrator.PostAsJsonAsync("/api/admin/raffle/draws", new { operationId = Guid.NewGuid(), expectedVersion = "6" });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private async Task EnterAsync(string email, string expectedVersion)
    {
        using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        var entry = await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt.OperationId, expectedVersion, input = new { email } });
        Assert.Equal(HttpStatusCode.OK, entry.StatusCode);
    }

    private sealed record ReceiptResponse(Guid OperationId);
}

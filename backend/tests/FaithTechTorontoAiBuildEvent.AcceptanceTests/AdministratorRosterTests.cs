// Acceptance Test
// Traces to: L2-002, L2-039
// Description: Only an authenticated administrator can read private participant roster details.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorRosterTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;
    public AdministratorRosterTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Administrator_reads_email_and_optional_profile_while_public_state_excludes_them()
    {
        using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt.OperationId, expectedVersion = "0", input = new { email = "private@example.com" } });
        await entrant.PutAsJsonAsync("/api/participant/profile", new { operationId = Guid.NewGuid(), expectedVersion = "1", input = new { name = "Private Name", whatYouMake = "Tools", onYourHeart = "Hope" } });

        var anonymous = await entrant.GetAsync("/api/admin/participants");
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        var roster = await administrator.GetFromJsonAsync<IReadOnlyList<RosterResponse>>("/api/admin/participants");

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.NotNull(roster);
        Assert.Contains(roster, participant => participant.Email == "private@example.com" && participant.Name == "Private Name" && participant.WhatYouMake == "Tools");
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record RosterResponse(string Email, string? Name, string? WhatYouMake);
}

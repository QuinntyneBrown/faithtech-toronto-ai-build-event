// Acceptance Test
// Traces to: L2-002
// Description: An administrator adds an email-only participant after team formation without creating a team assignment.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorParticipantAddTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;
    public AdministratorParticipantAddTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Administrator_adds_late_participant_unassigned()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "1", fromScreen = "projects", toScreen = "teams" });

        var created = await administrator.PostAsJsonAsync("/api/admin/participants", new { operationId = Guid.NewGuid(), expectedVersion = "2", email = "late@example.com" });
        var roster = await administrator.GetFromJsonAsync<IReadOnlyList<RosterResponse>>("/api/admin/participants");

        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        Assert.Contains(roster!, participant => participant.Email == "late@example.com" && participant.TeamLabel is null && !participant.HasWonRaffle);
    }

    private sealed record RosterResponse(string Email, string? TeamLabel, bool HasWonRaffle);
}

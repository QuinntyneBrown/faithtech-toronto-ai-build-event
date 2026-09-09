// Acceptance Test
// Traces to: L2-007, L2-010
// Description: Opening Team selection forms one persisted random grouping of current entrants in groups of three.

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class TeamFormationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public TeamFormationTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Projects_to_teams_forms_three_person_groups_with_final_remainder()
    {
        for (var index = 0; index < 4; index++)
        {
            using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
            var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
            Assert.NotNull(receipt);
            await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt.OperationId, expectedVersion = index.ToString(), input = new { email = $"team-{index}@example.com" } });
        }

        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "4", fromScreen = "countdown", toScreen = "projects" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "5", fromScreen = "projects", toScreen = "teams" });

        var state = await administrator.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.NotNull(state);
        Assert.Equal("teams", state.CurrentScreen);
        Assert.Equal(new[] { 1, 3 }, state.Teams.Select(team => team.Members.Count).Order());
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record PublicState(string CurrentScreen, IReadOnlyList<Team> Teams);
    private sealed record Team(IReadOnlyList<string> Members);
}

using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Roster;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class RosterProtectionTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-002;L2-038;L2-039;L2-040")]
    [InlineData("anonymous", HttpStatusCode.Unauthorized)]
    [InlineData("csrf", HttpStatusCode.BadRequest)]
    [InlineData("operation", HttpStatusCode.BadRequest)]
    [InlineData("missing-event", HttpStatusCode.NotFound)]
    [InlineData("blank", HttpStatusCode.UnprocessableEntity)]
    [InlineData("long-name", HttpStatusCode.UnprocessableEntity)]
    public async Task Given_an_invalid_roster_addition_when_submitted_then_no_participant_or_credential_is_created(string scenario, HttpStatusCode expected)
    {
        using var administrator = await factory.AdministratorBrowser(); var item = await Create(administrator);
        using var anonymous = factory.Browser(); var client = scenario == "anonymous" ? anonymous : administrator;
        if (scenario == "csrf") client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        var name = scenario == "blank" ? " \r\n " : scenario == "long-name" ? string.Concat(Enumerable.Repeat("😀", 201)) : "Alex";
        using var response = await Add(client, scenario == "missing-event" ? Guid.NewGuid() : item.Id, name, scenario == "operation" ? null : Guid.NewGuid());
        Assert.Equal(expected, response.StatusCode);
        Assert.Empty((await administrator.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!);
        if (scenario == "anonymous") {
            using var read = await anonymous.GetAsync($"/api/admin/events/{item.Id}/roster"); Assert.Equal(HttpStatusCode.Unauthorized, read.StatusCode);
        }
    }

    [Fact, Trait("Requirement", "L2-002/AC1;L2-040;L2-044")]
    public async Task Given_simultaneous_identical_additions_when_committed_then_one_identity_and_one_reveal_are_returned()
    {
        using var client = await factory.AdministratorBrowser(); var item = await Create(client); var operation = Guid.NewGuid();
        var name = " " + string.Concat(Enumerable.Repeat("😀", 200)) + " ";
        var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Add(client, item.Id, name, operation)));
        try {
            foreach (var response in responses) response.EnsureSuccessStatusCode();
            var results = await Task.WhenAll(responses.Select(response => response.Content.ReadFromJsonAsync<RosterIssuance>()));
            Assert.Single(results.Select(x => x!.Entry.Id).Distinct()); Assert.Single(results, x => x!.Code is not null);
            Assert.Equal(3, results.Count(x => x!.PreviouslyCompleted));
            Assert.Single((await client.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!);
            Assert.All(results, result => Assert.Equal(name.Trim(), result!.Entry.DisplayName));
        } finally { foreach (var response in responses) response.Dispose(); }
    }

    [Fact, Trait("Requirement", "L2-002;L2-040")]
    public async Task Given_a_client_supplied_entry_code_when_a_participant_is_added_then_the_server_issues_its_own_credential()
    {
        using var client = await factory.AdministratorBrowser(); var item = await Create(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{item.Id}/roster") {
            Content = JsonContent.Create(new { displayName = "Alex", code = "CHOSEN-BY-CLIENT", active = false }) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.SendAsync(request); response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<RosterIssuance>())!;
        Assert.NotEqual("CHOSEN-BY-CLIENT", result.Code); Assert.True(result.Entry.Active);
    }
    private static async Task<EventSummary> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Roster protection" }); response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); return (await response.Content.ReadFromJsonAsync<EventSummary>())!;
    }
    private static async Task<HttpResponseMessage> Add(HttpClient client, Guid eventId, string name, Guid? operation)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{eventId}/roster") { Content = JsonContent.Create(new { displayName = name }) };
        if (operation is not null) request.Headers.Add("Idempotency-Key", operation.ToString()); return await client.SendAsync(request);
    }
}

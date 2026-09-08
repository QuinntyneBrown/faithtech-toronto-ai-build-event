using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ReferenceScheduleProtectionTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-038;L2-039;L2-044")]
    [InlineData("anonymous", HttpStatusCode.Unauthorized)]
    [InlineData("csrf", HttpStatusCode.BadRequest)]
    [InlineData("version", HttpStatusCode.PreconditionRequired)]
    [InlineData("stale", HttpStatusCode.Conflict)]
    public async Task Given_a_reference_request_without_required_protection_when_applied_then_the_draft_is_unchanged(string scenario, HttpStatusCode expected)
    {
        using var administrator = await factory.AdministratorBrowser(); var item = await Create(administrator);
        using var anonymous = factory.Browser();
        var client = scenario == "anonymous" ? anonymous : administrator;
        if (scenario == "csrf") client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        using var response = await Apply(client, item.Id, scenario == "version" ? null : scenario == "stale" ? "stale" : item.Version, Guid.NewGuid());
        Assert.Equal(expected, response.StatusCode);
        var current = (await administrator.GetFromJsonAsync<EventDetail>($"/api/admin/events/{item.Id}"))!;
        Assert.Equal(item.Version, current.Version); Assert.Null(current.VenueName); Assert.Null(current.Start);
        Assert.Empty((await administrator.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{item.Id}/schedule"))!.Stages);
    }

    [Fact, Trait("Requirement", "L2-008/AC5;L2-044/AC6")]
    public async Task Given_two_reference_events_when_one_is_edited_and_its_original_application_retried_then_the_other_is_unchanged_and_no_second_audit_is_written()
    {
        using var client = await factory.AdministratorBrowser(); var first = await Create(client); var second = await Create(client);
        var operation = Guid.NewGuid();
        using var firstResponse = await Apply(client, first.Id, first.Version, operation); firstResponse.EnsureSuccessStatusCode();
        using var secondResponse = await Apply(client, second.Id, second.Version, Guid.NewGuid()); secondResponse.EnsureSuccessStatusCode();
        var original = (await firstResponse.Content.ReadFromJsonAsync<ScheduleDetail>())!;
        var other = (await secondResponse.Content.ReadFromJsonAsync<ScheduleDetail>())!;
        Assert.Empty(original.Stages.Select(x => x.Id).Intersect(other.Stages.Select(x => x.Id)));
        var input = new ScheduleInput(original.Timezone, original.Start, original.End,
            original.Stages.Select((stage, index) => index == 0 ? stage with { Name = "Edited arrival" } : stage).ToArray(), original.Selection, original.Presentation);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{first.Id}/schedule") { Content = JsonContent.Create(input) };
        request.Headers.Add("If-Match", $"\"{original.Version}\""); request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var edited = await client.SendAsync(request); edited.EnsureSuccessStatusCode();
        using var retry = await Apply(client, first.Id, first.Version, operation); retry.EnsureSuccessStatusCode();
        Assert.Equal(await firstResponse.Content.ReadAsStringAsync(), await retry.Content.ReadAsStringAsync());
        Assert.Equal("Edited arrival", (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{first.Id}/schedule"))!.Stages[0].Name);
        Assert.Equal(other.Stages[0], (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{second.Id}/schedule"))!.Stages[0]);
        using var scope = factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        Assert.Equal(1, await db.AuditRecords.CountAsync(x => x.EventId == first.Id && x.Action == "reference-schedule-applied"));
        using var changed = await Apply(client, first.Id, original.Version, operation);
        Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
    }

    private static async Task<EventSummary> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Reference isolation" }); response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); return (await response.Content.ReadFromJsonAsync<EventSummary>())!;
    }
    private static async Task<HttpResponseMessage> Apply(HttpClient client, Guid id, string? version, Guid operation)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{id}/reference-schedule");
        if (version is not null) request.Headers.Add("If-Match", $"\"{version}\"");
        request.Headers.Add("Idempotency-Key", operation.ToString()); return await client.SendAsync(request);
    }
}

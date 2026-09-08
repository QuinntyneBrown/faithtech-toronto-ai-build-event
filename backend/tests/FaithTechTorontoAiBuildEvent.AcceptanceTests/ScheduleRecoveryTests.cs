using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ScheduleRecoveryTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-006;L2-044/AC4;L2-044/AC6")]
    public async Task Given_a_saved_schedule_when_retried_and_edited_then_receipts_and_stage_identities_are_preserved()
    {
        using var client = await factory.AdministratorBrowser();
        var initial = await Create(client);
        var input = Input(); var key = Guid.NewGuid().ToString();
        using var first = await Save(client, initial, input, key); first.EnsureSuccessStatusCode();
        var saved = (await first.Content.ReadFromJsonAsync<ScheduleDetail>())!;
        var edited = input with { Stages = [input.Stages![0] with { Name = "Updated guidance" }] };
        using var second = await Save(client, saved, edited); second.EnsureSuccessStatusCode();
        var latest = (await second.Content.ReadFromJsonAsync<ScheduleDetail>())!;
        Assert.Equal(input.Stages[0].Id, latest.Stages[0].Id);
        Assert.Equal("Updated guidance", latest.Stages[0].Name);
        using var retry = await Save(client, initial, input, key);
        Assert.Equal(await first.Content.ReadAsStringAsync(), await retry.Content.ReadAsStringAsync());
        using var changed = await Save(client, initial, edited, key);
        Assert.Equal(HttpStatusCode.Conflict, changed.StatusCode);
        using var stale = await Save(client, initial, edited);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var removed = await Save(client, latest, edited with { Stages = [] }); removed.EnsureSuccessStatusCode();
        Assert.Empty((await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{latest.Id}/schedule"))!.Stages);
    }

    [Fact, Trait("Requirement", "L2-001/AC6;L2-006/AC1")]
    public async Task Given_a_saved_stage_when_event_settings_shorten_its_parent_interval_then_the_edit_is_rejected_without_partial_changes()
    {
        using var client = await factory.AdministratorBrowser();
        var initial = await Create(client); var input = Input();
        using var first = await Save(client, initial, input); first.EnsureSuccessStatusCode();
        var saved = (await first.Content.ReadFromJsonAsync<ScheduleDetail>())!;
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{saved.Id}") {
            Content = JsonContent.Create(new { title = "Must not be saved", timezone = "UTC", start = input.Start, end = Time(23, 30) }) };
        request.Headers.Add("If-Match", $"\"{saved.Version}\""); request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var detail = (await client.GetFromJsonAsync<EventDetail>($"/api/admin/events/{saved.Id}"))!;
        Assert.Equal("Schedule recovery", detail.Title); Assert.Equal(saved.Version, detail.Version);
        Assert.Single((await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{saved.Id}/schedule"))!.Stages);
    }

    private static LocalTimeInput Time(int hour, int minute = 0) => new(new DateTime(2026, 9, 9, hour, minute, 0), 0);
    private static ScheduleInput Input() => new("UTC", Time(23), new(new DateTime(2026, 9, 10, 1, 0, 0), 0),
        [new(Guid.NewGuid(), "Guidance", "Develop", "information", "Original content", null, Time(23), new(new DateTime(2026, 9, 10, 0, 0, 0), 0))], null, null);
    private static async Task<ScheduleDetail> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Schedule recovery" }); response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); var item = (await response.Content.ReadFromJsonAsync<EventSummary>())!;
        return (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{item.Id}/schedule"))!;
    }
    private static async Task<HttpResponseMessage> Save(HttpClient client, ScheduleDetail current, ScheduleInput input, string? operationId = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{current.Id}/schedule") { Content = JsonContent.Create(input) };
        request.Headers.Add("If-Match", $"\"{current.Version}\""); request.Headers.Add("Idempotency-Key", operationId ?? Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }
}

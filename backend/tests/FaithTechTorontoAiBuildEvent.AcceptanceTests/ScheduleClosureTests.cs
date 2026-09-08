using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ScheduleClosureTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-006/AC7;L2-044")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_an_open_or_elapsed_selection_window_when_disabled_then_reenabling_cannot_reopen_it(bool alreadyElapsed)
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Published(client, alreadyElapsed, false);
        var disabled = Configuration(original) with { Selection = null };
        using var disable = await Save(client, original, disabled); disable.EnsureSuccessStatusCode();
        var closed = (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{original.Id}/schedule"))!;
        Assert.True(closed.SelectionClosed);
        using var reopen = await Save(client, closed, disabled with { Selection = new(original.Start, original.End) });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reopen.StatusCode);
        Assert.True((await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{original.Id}/schedule"))!.SelectionClosed);
    }

    [Fact, Trait("Requirement", "L2-006/AC8")]
    public async Task Given_a_published_event_that_ended_without_a_worker_when_its_end_moves_forward_then_reopening_is_rejected()
    {
        using var client = await factory.AdministratorBrowser();
        var original = await Published(client, true, true);
        Assert.True(original.Completed);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{original.Id}") {
            Content = JsonContent.Create(new { title = "Reopening attempt", timezone = "UTC", start = original.Start,
                end = new LocalTimeInput(DateTime.SpecifyKind(DateTime.UtcNow.AddHours(1), DateTimeKind.Unspecified), 0) }) };
        request.Headers.Add("If-Match", $"\"{original.Version}\""); request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var current = (await client.GetFromJsonAsync<EventDetail>($"/api/admin/events/{original.Id}"))!;
        Assert.Equal("Closure acceptance", current.Title); Assert.Equal(original.Version, current.Version);
        using var correction = await Save(client, original, Configuration(original)); correction.EnsureSuccessStatusCode();
        Assert.True((await correction.Content.ReadFromJsonAsync<ScheduleDetail>())!.Completed);
    }

    private async Task<ScheduleDetail> Published(HttpClient client, bool selectionElapsed, bool eventElapsed)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Closure acceptance" }); response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); var item = (await response.Content.ReadFromJsonAsync<EventSummary>())!;
        using var scope = factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var now = await db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value").SingleAsync();
        LocalTimeInput Time(int minutes) => new(DateTime.SpecifyKind(now.UtcDateTime.AddMinutes(minutes), DateTimeKind.Unspecified), 0);
        var draft = (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{item.Id}/schedule"))!;
        var input = new ScheduleInput("UTC", Time(-120), Time(eventElapsed ? -5 : 120), [], new(Time(-60), Time(selectionElapsed ? -10 : 60)), null);
        using var saved = await Save(client, draft, input); saved.EnsureSuccessStatusCode();
        var entity = await db.Events.SingleAsync(x => x.Id == item.Id); entity.Published = true; await db.SaveChangesAsync();
        return (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{item.Id}/schedule"))!;
    }
    private static ScheduleInput Configuration(ScheduleDetail detail) => new(detail.Timezone, detail.Start, detail.End, detail.Stages, detail.Selection, detail.Presentation);
    private static async Task<HttpResponseMessage> Save(HttpClient client, ScheduleDetail current, ScheduleInput input)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{current.Id}/schedule") { Content = JsonContent.Create(input) };
        request.Headers.Add("If-Match", $"\"{current.Version}\""); request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }
}

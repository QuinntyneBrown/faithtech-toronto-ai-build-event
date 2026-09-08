using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ReferenceScheduleTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-008/AC1;L2-008/AC2;L2-008/AC4;L2-008/AC6;L2-044/AC6")]
    public async Task Given_a_blank_event_when_the_reference_is_applied_then_all_twenty_one_entries_are_editable_draft_content_with_durable_retry()
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "Reference acceptance" }); created.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); var item = (await created.Content.ReadFromJsonAsync<EventSummary>())!;
        var key = Guid.NewGuid().ToString();
        async Task<HttpResponseMessage> Apply() {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{item.Id}/reference-schedule");
            request.Headers.Add("If-Match", $"\"{item.Version}\""); request.Headers.Add("Idempotency-Key", key);
            return await client.SendAsync(request);
        }
        using var response = await Apply(); response.EnsureSuccessStatusCode();
        var schedule = (await response.Content.ReadFromJsonAsync<ScheduleDetail>())!;
        var starts = new[] { "17:00", "17:20", "17:22", "17:28", "17:40", "17:45", "17:50", "17:55", "18:05", "18:10", "18:15", "18:30", "18:40", "19:10", "19:20", "19:35", "19:45", "20:10", "20:30", "20:50", "20:55" };
        Assert.Equal(starts, schedule.Stages.Select(x => x.Start!.Local.ToString("HH:mm")));
        Assert.Equal(21, schedule.Stages.Select(x => x.Id).Distinct().Count());
        Assert.Equal("America/Toronto", schedule.Timezone); Assert.Equal(Time(17), schedule.Start); Assert.Equal(Time(21), schedule.End);
        Assert.Equal(new WindowInput(Time(18, 5), Time(18, 15)), schedule.Selection);
        Assert.Equal(new WindowInput(Time(20, 30), Time(20, 50)), schedule.Presentation);
        for (var index = 0; index < schedule.Stages.Count; index++) {
            Assert.Equal(index == 20 ? schedule.End : schedule.Stages[index + 1].Start, schedule.Stages[index].End);
            Assert.False(string.IsNullOrWhiteSpace(schedule.Stages[index].Content));
            Assert.NotEqual("quiz", schedule.Stages[index].ScreenType);
        }
        Assert.Equal("Gather", schedule.Stages[0].Phase); Assert.Equal("Discover", schedule.Stages[3].Phase);
        Assert.Equal("Discern", schedule.Stages[8].Phase); Assert.Equal("Develop", schedule.Stages[10].Phase);
        Assert.Equal("Demonstrate", schedule.Stages[17].Phase); Assert.Equal("Send", schedule.Stages[19].Phase);
        Assert.Contains("prayer", schedule.Stages[1].Content!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pizza", schedule.Stages[12].Content!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("network", schedule.Stages[14].Content!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("20:00", schedule.Stages[16].Content!);
        Assert.Contains("60 seconds", schedule.Stages[17].Content!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("demos", schedule.Stages[18].ScreenType); Assert.Equal("raffle", schedule.Stages[19].ScreenType);
        var detail = (await client.GetFromJsonAsync<EventDetail>($"/api/admin/events/{item.Id}"))!;
        Assert.Equal("Stone Church", detail.VenueName); Assert.False(detail.Published); Assert.False(detail.UseLiturgy);
        Assert.Null(detail.Address); Assert.Null(detail.Latitude); Assert.Null(detail.Longitude); Assert.Null(detail.Logo);
        using var retry = await Apply(); retry.EnsureSuccessStatusCode();
        Assert.Equal(await response.Content.ReadAsStringAsync(), await retry.Content.ReadAsStringAsync());
        var edited = new ScheduleInput(schedule.Timezone, schedule.Start, schedule.End,
            schedule.Stages.Select((stage, index) => index == 0 ? stage with { Content = "Administrator correction" } : stage).ToArray(), schedule.Selection, schedule.Presentation);
        using var save = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{item.Id}/schedule") { Content = JsonContent.Create(edited) };
        save.Headers.Add("If-Match", $"\"{schedule.Version}\""); save.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var saved = await client.SendAsync(save); saved.EnsureSuccessStatusCode();
        Assert.Equal("Administrator correction", (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{item.Id}/schedule"))!.Stages[0].Content);
    }
    private static LocalTimeInput Time(int hour, int minute = 0) => new(new DateTime(2026, 9, 9, hour, minute, 0), -240);
}

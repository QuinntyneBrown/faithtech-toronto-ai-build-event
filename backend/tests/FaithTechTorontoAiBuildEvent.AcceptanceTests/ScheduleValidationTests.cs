using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ScheduleValidationTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-006/AC1;L2-006/AC4;L2-040")]
    [InlineData("before", "stages[0].start")]
    [InlineData("after", "stages[0].end")]
    [InlineData("zero", "stages[0].end")]
    [InlineData("identity", "stages[1].id")]
    [InlineData("zone", "timezone")]
    [InlineData("ambiguous", "stages[0].start")]
    [InlineData("nonexistent", "stages[0].start")]
    [InlineData("offset", "stages[0].start")]
    [InlineData("selection", "selection.end")]
    [InlineData("presentation", "presentation.start")]
    [InlineData("url", "stages[0].resourceUrl")]
    [InlineData("name", "stages[0].name")]
    public async Task Given_invalid_schedule_content_when_saved_then_the_affected_field_is_identified_and_nothing_changes(string scenario, string field)
    {
        using var client = await factory.AdministratorBrowser();
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var created = await client.PostAsJsonAsync("/api/admin/events", new { title = "Validation acceptance" });
        created.EnsureSuccessStatusCode(); client.DefaultRequestHeaders.Remove("Idempotency-Key");
        var item = (await created.Content.ReadFromJsonAsync<EventSummary>())!;
        var original = (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{item.Id}/schedule"))!;
        var input = Invalid(scenario);
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{item.Id}/schedule") { Content = JsonContent.Create(input) };
        request.Headers.Add("If-Match", $"\"{original.Version}\""); request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out var messages), problem.ToString());
        Assert.NotEmpty(messages.EnumerateArray());
        var current = (await client.GetFromJsonAsync<ScheduleDetail>($"/api/admin/events/{item.Id}/schedule"))!;
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(current));
    }

    private static ScheduleInput Invalid(string scenario)
    {
        LocalTimeInput Time(int hour) => new(new DateTime(2026, 9, 9, hour, 0, 0), 0);
        var stage = new StageInput(Guid.NewGuid(), "Guidance", null, "information", null, null, Time(18), Time(19));
        var input = new ScheduleInput("UTC", Time(17), Time(21), [stage], null, null);
        if (scenario is "ambiguous" or "nonexistent") {
            var day = scenario == "ambiguous" ? new DateTime(2026, 11, 1) : new DateTime(2026, 3, 8);
            return input with { Timezone = "America/Toronto", Start = new(day, null), End = new(day.AddHours(5), null),
                Stages = [stage with { Start = new(day.AddHours(scenario == "ambiguous" ? 1.5 : 2.5), scenario == "ambiguous" ? null : -300), End = new(day.AddHours(4), null) }] };
        }
        return scenario switch {
            "before" => input with { Stages = [stage with { Start = Time(16) }] },
            "after" => input with { Stages = [stage with { End = Time(22) }] },
            "zero" => input with { Stages = [stage with { End = stage.Start }] },
            "identity" => input with { Stages = [stage, stage] },
            "zone" => input with { Timezone = "Unknown/Timezone" },
            "offset" => input with { Stages = [stage with { Start = stage.Start! with { OffsetMinutes = 60 } }] },
            "selection" => input with { Selection = new(Time(18), Time(22)) },
            "presentation" => input with { Presentation = new(Time(16), Time(20)) },
            "url" => input with { Stages = [stage with { ResourceUrl = "https://person:secret@example.org" }] },
            "name" => input with { Stages = [stage with { Name = " " }] },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
    }
}

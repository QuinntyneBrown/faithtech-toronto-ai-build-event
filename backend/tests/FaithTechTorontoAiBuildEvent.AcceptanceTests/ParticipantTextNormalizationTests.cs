// Acceptance Test
// Traces to: L2-013, L2-040
// Description: Optional participant text is trimmed and persisted with canonical LF line endings.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantTextNormalizationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ParticipantTextNormalizationTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Saved_optional_text_is_trimmed_and_uses_lf_line_endings()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await client.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        await client.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt!.OperationId, expectedVersion = "0", input = new { email = "normalize@example.com" } });

        var saved = await client.PutAsJsonAsync("/api/participant/profile", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "1",
            input = new { name = "  Name  ", whatYouMake = "  First\r\nSecond  ", onYourHeart = "  Third\rFourth  " }
        });
        var profile = await client.GetFromJsonAsync<ProfileResponse>("/api/participant/profile");

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        Assert.Equal("Name", profile!.Name);
        Assert.Equal("First\nSecond", profile.WhatYouMake);
        Assert.Equal("Third\nFourth", profile.OnYourHeart);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record ProfileResponse(string? Name, string? WhatYouMake, string? OnYourHeart);
}

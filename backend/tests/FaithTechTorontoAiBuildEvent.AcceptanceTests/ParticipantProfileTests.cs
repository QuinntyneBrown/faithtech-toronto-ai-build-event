// Acceptance Test
// Traces to: L2-013, L2-039, L2-040
// Description: An entered participant saves optional introduction fields and restores them through their private session.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantProfileTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ParticipantProfileTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Owner_saves_and_restores_optional_profile_fields()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receiptResponse = await client.PostAsync("/api/participant/entry-receipt", null);
        var receipt = await receiptResponse.Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        await client.PostAsJsonAsync("/api/participant/entries", new
        {
            operationId = receipt.OperationId,
            expectedVersion = "0",
            input = new { email = "profile-owner@example.com" }
        });

        var save = await client.PutAsJsonAsync("/api/participant/profile", new
        {
            operationId = Guid.NewGuid(),
            expectedVersion = "1",
            input = new { name = "Taylor", whatYouMake = "Software", onYourHeart = "Belonging" }
        });
        var profile = await client.GetFromJsonAsync<ProfileResponse>("/api/participant/profile");

        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        Assert.NotNull(profile);
        Assert.Equal("Taylor", profile.Name);
        Assert.Equal("Software", profile.WhatYouMake);
        Assert.Equal("Belonging", profile.OnYourHeart);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record ProfileResponse(string? Name, string? WhatYouMake, string? OnYourHeart);
}

// Acceptance Test
// Traces to: L2-040
// Description: Optional profile limits count Unicode scalar values rather than UTF-16 code units.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class UnicodeProfileValidationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public UnicodeProfileValidationTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Profile_accepts_200_unicode_scalars_and_rejects_201()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await client.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        await client.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt!.OperationId, expectedVersion = "0", input = new { email = "unicode@example.com" } });
        var acceptedName = string.Concat(Enumerable.Repeat("😀", 200));

        var accepted = await client.PutAsJsonAsync("/api/participant/profile", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "1", input = new { name = acceptedName, whatYouMake = (string?)null, onYourHeart = (string?)null }
        });
        var rejected = await client.PutAsJsonAsync("/api/participant/profile", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "2", input = new { name = acceptedName + "😀", whatYouMake = (string?)null, onYourHeart = (string?)null }
        });
        var profile = await client.GetFromJsonAsync<ProfileResponse>("/api/participant/profile");

        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Equal(acceptedName, profile!.Name);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record ProfileResponse(string? Name);
}

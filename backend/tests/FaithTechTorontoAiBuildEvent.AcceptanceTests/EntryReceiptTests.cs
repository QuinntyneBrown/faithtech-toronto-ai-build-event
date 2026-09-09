// Acceptance Test
// Traces to: L2-004, L2-041, L2-044
// Description: An anonymous browser obtains a protected pre-submission receipt without exposing its credential in the response.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EntryReceiptTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public EntryReceiptTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Entry_receipt_is_an_http_only_cookie_and_response_contains_only_operation_identity()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsync("/api/participant/entry-receipt", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var receipt = await response.Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        Assert.NotEqual(Guid.Empty, receipt.OperationId);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.Contains("faithtech-entry-receipt=", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Existing_unexpired_receipt_reuses_its_operation_identity()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var first = await client.PostAsync("/api/participant/entry-receipt", null);
        var second = await client.PostAsync("/api/participant/entry-receipt", null);

        var firstReceipt = await first.Content.ReadFromJsonAsync<ReceiptResponse>();
        var secondReceipt = await second.Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(firstReceipt);
        Assert.NotNull(secondReceipt);
        Assert.Equal(firstReceipt.OperationId, secondReceipt.OperationId);
    }

    private sealed record ReceiptResponse(Guid OperationId);
}

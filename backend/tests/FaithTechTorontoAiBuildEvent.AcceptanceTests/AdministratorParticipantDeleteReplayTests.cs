// Acceptance Test
// Traces to: L2-014, L2-039, L2-044
// Description: Participant deletion retries succeed without another deletion and remove retained private mutation outcomes.

using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorParticipantDeleteReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdministratorParticipantDeleteReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_delete_retry_succeeds_and_private_receipt_results_are_removed()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        var added = await client.PostAsJsonAsync("/api/admin/participants", new { operationId = Guid.NewGuid(), expectedVersion = "0", email = "delete-replay@example.com" });
        var participant = await added.Content.ReadFromJsonAsync<ParticipantResponse>();
        await client.PutAsJsonAsync($"/api/admin/participants/{participant!.Id}", new
        {
            operationId = Guid.NewGuid(),
            expectedVersion = "1",
            input = new { email = participant.Email, name = "Private name", whatYouMake = "Private answer", onYourHeart = (string?)null }
        });
        var operationId = Guid.NewGuid();
        var request = new { operationId, expectedVersion = "2" };

        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(Delete($"/api/admin/participants/{participant.Id}", request))).StatusCode);
        var replay = await client.SendAsync(Delete($"/api/admin/participants/{participant.Id}", request));
        var changed = await client.SendAsync(Delete($"/api/admin/participants/{Guid.NewGuid()}", new { operationId, expectedVersion = "3" }));
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
        var receipts = await database.EventOperationReceipts.AsNoTracking().ToListAsync();

        Assert.Equal(HttpStatusCode.NoContent, replay.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.DoesNotContain(receipts, receipt => receipt.ResultId == participant.Id && receipt.ResultJson is not null);
        Assert.Contains(receipts, receipt => receipt.OperationId == operationId && receipt.OperationKind == "delete-administrator-participant");
    }

    private static HttpRequestMessage Delete(string path, object body) => new(HttpMethod.Delete, path) { Content = JsonContent.Create(body) };

    private sealed record ParticipantResponse(Guid Id, string Email);
}

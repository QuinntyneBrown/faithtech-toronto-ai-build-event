// Acceptance Test: L2-058/059. Operator results never replay as application results.
using FaithTechTorontoAiBuildEvent.Domain.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class OperatorAttributionTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact]
    public async Task Given_an_operator_receipt_when_the_application_reads_outcomes_then_only_its_own_result_is_visible()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var actor = Guid.NewGuid(); var operation = Guid.NewGuid();
        foreach (var kind in new[] { ActorKind.Application, ActorKind.DatabaseOperator })
            db.OperationReceipts.Add(new() { ActorId = actor, ActorKind = kind, OperationId = operation,
                Target = "test", PayloadHash = "hash", Result = kind.ToString(), CommittedAtUtc = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        Assert.Equal("Application", (await db.OperationReceipts.SingleAsync(x => x.ActorId == actor)).Result);
        Assert.Equal(2, await db.OperationReceipts.IgnoreQueryFilters().CountAsync(x => x.ActorId == actor));
    }
}

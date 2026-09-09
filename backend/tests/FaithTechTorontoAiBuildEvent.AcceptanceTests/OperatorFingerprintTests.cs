// Acceptance Test: L2-058. Retry identity includes field presence but ignores JSON property order.
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;
namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class OperatorFingerprintTests
{
    [Fact]
    public void Given_equivalent_property_order_or_changed_presence_when_fingerprinted_then_only_the_changed_intent_conflicts()
    {
        var first = EventSeedReader.Read("{\"event\":{\"title\":null,\"address\":null}}"u8.ToArray());
        var reordered = EventSeedReader.Read("{\"event\":{\"address\":null,\"title\":null}}"u8.ToArray());
        var changed = EventSeedReader.Read("{\"event\":{\"title\":null}}"u8.ToArray());
        var after = EventSeedMerge.Merge(first, new(null, null, null, null, null, null, null, null), new(null, null, null, [], null, null), false);
        var review = new EventImportReview(Guid.NewGuid(), false, "version", first, null, after, false);
        Assert.Equal(SqlOperatorUnitOfWork.Hash(review), SqlOperatorUnitOfWork.Hash(review with { Seed = reordered }));
        Assert.NotEqual(SqlOperatorUnitOfWork.Hash(review), SqlOperatorUnitOfWork.Hash(review with { Seed = changed }));
    }
}

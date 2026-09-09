// Acceptance Test: L2-054. Given a seed, parsing preserves input and rejects ambiguous documents.
using System.Text;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Validation;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventSeedReaderTests
{
    [Theory]
    [InlineData("{\"event\":{},\"event\":{}}")]
    [InlineData("{\"event\":{\"title\":\"a\",\"title\":\"b\"}}")]
    [InlineData("{\"event\":{\"published\":true}}")]
    [InlineData("{\"event\":null}")]
    [InlineData("{\"schedule\":{}}")]
    [InlineData("{\"source\":{}}")]
    [InlineData("{\"Event\":{}}")]
    public void Given_ambiguous_or_invalid_seed_when_read_then_it_is_rejected(string json)
        => Assert.Throws<InputValidationException>(() => EventSeedReader.Read(Encoding.UTF8.GetBytes(json)));

    [Fact]
    public void Given_metadata_and_explicit_null_when_read_then_metadata_is_inert_and_presence_is_preserved()
    {
        var seed = EventSeedReader.Read(Encoding.UTF8.GetBytes("\uFEFF{\"event\":{\"title\":null},\"source\":\"C:/never-open\"}"));
        Assert.True(seed.Event!.Value.TryGetProperty("title", out var title));
        Assert.Equal(System.Text.Json.JsonValueKind.Null, title.ValueKind);
    }
}

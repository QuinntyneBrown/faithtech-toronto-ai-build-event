// Acceptance Test: L2-057/059. Reviewed input survives restart without plaintext storage.
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class OperatorArtifactTests
{
    [Fact]
    public void Given_a_saved_preview_when_reopened_then_the_original_input_is_recovered_without_plaintext_on_disk()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try {
            var store = new ProtectedOperatorFiles(path);
            store.Write("preview", "synthetic-private-content");
            Assert.Equal("synthetic-private-content", new ProtectedOperatorFiles(path).Read<string>("preview"));
            Assert.DoesNotContain("synthetic-private-content", File.ReadAllText(Path.Combine(path, "preview")));
            Assert.Throws<ArgumentException>(() => store.Write("../outside", "value"));
        } finally { if (Directory.Exists(path)) Directory.Delete(path, true); }
    }
}

using FaithTechTorontoAiBuildEvent.Provisioning;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class OperatorTargetProfileTests
{
    [Fact, Trait("Requirement", "L2-050/AC1")]
    public void Given_a_named_target_when_saved_then_it_is_loaded_without_a_connection_string()
    {
        var path = Path.Combine(Path.GetTempPath(), $"target-{Guid.NewGuid():N}.json");
        var store = new OperatorTargetProfileStore(path);

        store.Save("production", new OperatorTargetProfile("server.database.windows.net", "FaithTech", "production"));

        Assert.Equal("FaithTech", store.Load("production").Database);
        Assert.DoesNotContain("Password", File.ReadAllText(path), StringComparison.OrdinalIgnoreCase);
    }
}

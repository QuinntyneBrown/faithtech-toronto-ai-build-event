namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProvisioningCommandTests
{
    [Fact, Trait("Requirement", "L2-049/AC1")]
    public async Task Given_help_when_the_tool_is_invoked_then_it_returns_success_without_opening_a_database_connection()
    {
        var exitCode = await Provisioning.Program.Main(["--help"]);

        Assert.Equal(0, exitCode);
    }
}

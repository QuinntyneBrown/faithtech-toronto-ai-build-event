namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProvisioningCommandTests
{
    [Fact, Trait("Requirement", "L2-049/AC1")]
    public async Task Given_help_when_the_tool_is_invoked_then_it_returns_success_without_opening_a_database_connection()
    {
        var exitCode = await Provisioning.Program.Main(["--help"]);

        Assert.Equal(0, exitCode);
    }

    [Theory, Trait("Requirement", "L2-049/AC1")]
    [InlineData("migrate")]
    [InlineData("create-admin")]
    [InlineData("disable-admin")]
    public async Task Given_a_deployment_command_when_help_is_requested_then_no_database_is_required(string command)
    {
        Assert.Equal(0, await Provisioning.Program.Main([command, "--help"]));
    }

    [Theory, Trait("Requirement", "L2-049/AC4")]
    [InlineData("create-admin")]
    [InlineData("disable-admin")]
    [InlineData("unknown")]
    public async Task Given_invalid_syntax_when_invoked_then_the_command_fails_before_execution(string command)
    {
        Assert.NotEqual(0, await Provisioning.Program.Main([command]));
    }
}

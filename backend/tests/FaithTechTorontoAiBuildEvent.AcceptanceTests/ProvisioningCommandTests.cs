// Acceptance Test
// Traces to: L2-049, L2-061, L2-062
// Description: Help and invalid CLI syntax do not require a database.
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
    [InlineData("add-user")]
    [InlineData("reset-password")]
    public async Task Given_a_deployment_command_when_help_is_requested_then_no_database_is_required(string command)
    {
        Assert.Equal(0, await Provisioning.Program.Main([command, "--help"]));
    }

    [Theory, Trait("Requirement", "L2-049/AC4")]
    [InlineData("create-admin")]
    [InlineData("disable-admin")]
    [InlineData("unknown")]
    [InlineData("add-user")]
    [InlineData("reset-password")]
    public async Task Given_invalid_syntax_when_invoked_then_the_command_fails_before_execution(string command)
    {
        Assert.NotEqual(0, await Provisioning.Program.Main([command]));
    }

    [Theory, Trait("Requirement", "L2-061/AC3;L2-062/AC5")]
    [InlineData("add-user test --prompt-password --password-stdin")]
    [InlineData("reset-password test --all --password-stdin")]
    [InlineData("reset-password --password-stdin")]
    [InlineData("reset-password test")]
    [InlineData("reset-password --all")]
    public async Task Given_conflicting_or_missing_options_when_parsed_then_execution_does_not_start(string arguments)
    {
        Assert.NotEqual(0, await Provisioning.Program.Main(arguments.Split(' ')));
    }
}

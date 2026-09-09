using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class VerifyConnectionCommand
{
    public static Command Create()
    {
        var target = new Option<string>("--target") { Required = true, Description = "Explicit connection-string environment target name." };
        var connectionTimeout = new Option<int?>("--connection-timeout-seconds") { Description = "Positive connection timeout in seconds (default: 30)." };
        var commandTimeout = new Option<int?>("--command-timeout-seconds") { Description = "Positive command timeout in seconds (default: 60)." };
        var command = new Command("verify-connection", "Verify an explicitly selected encrypted SQL Server connection.");
        command.Options.Add(target);
        command.Options.Add(connectionTimeout);
        command.Options.Add(commandTimeout);
        command.SetAction(parseResult => OperatorDatabase.VerifyConnectionAsync(parseResult.GetValue(target) ?? string.Empty, parseResult.GetValue(connectionTimeout), parseResult.GetValue(commandTimeout)));
        return command;
    }
}

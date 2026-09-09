using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class VerifyConnectionCommand
{
    public static Command Create()
    {
        var target = new Option<string>("--target") { Required = true, Description = "Explicit connection-string environment target name." };
        var command = new Command("verify-connection", "Verify an explicitly selected encrypted SQL Server connection.");
        command.Options.Add(target);
        command.SetAction(parseResult => OperatorDatabase.VerifyConnectionAsync(parseResult.GetValue(target) ?? string.Empty));
        return command;
    }
}

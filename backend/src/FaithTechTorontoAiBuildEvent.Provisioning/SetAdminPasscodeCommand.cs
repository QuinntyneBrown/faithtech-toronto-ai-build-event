using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class SetAdminPasscodeCommand
{
    public static Command Create()
    {
        var target = new Option<string>("--target") { Required = true, Description = "Explicit connection-string environment target name." };
        var stdin = new Option<bool>("--passcode-stdin") { Description = "Read the four-digit passcode from standard input." };
        var command = new Command("set-admin-passcode", "Replace the administrator passcode without printing it.");
        command.Options.Add(target);
        command.Options.Add(stdin);
        command.SetAction(async parseResult =>
        {
            if (!parseResult.GetValue(stdin))
            {
                Console.Error.WriteLine("Use --passcode-stdin to provide the passcode without command-line exposure.");
                return 2;
            }
            var passcode = await Console.In.ReadLineAsync();
            return await OperatorDatabase.ReplacePasscodeAsync(parseResult.GetValue(target) ?? string.Empty, passcode);
        });
        return command;
    }
}

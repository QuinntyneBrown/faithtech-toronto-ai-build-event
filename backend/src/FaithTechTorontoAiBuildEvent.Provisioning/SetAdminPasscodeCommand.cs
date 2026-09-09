using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class SetAdminPasscodeCommand
{
    public static Command Create()
    {
        var target = new Option<string>("--target") { Required = true, Description = "Explicit connection-string environment target name." };
        var stdin = new Option<bool>("--passcode-stdin") { Description = "Read the four-digit passcode from standard input." };
        var interactive = new Option<bool>("--interactive") { Description = "Read the four-digit passcode through a masked prompt." };
        var command = new Command("set-admin-passcode", "Replace the administrator passcode without printing it.");
        command.Options.Add(target);
        command.Options.Add(stdin);
        command.Options.Add(interactive);
        command.SetAction(async parseResult =>
        {
            var useStdin = parseResult.GetValue(stdin);
            var useInteractive = parseResult.GetValue(interactive);
            if (useStdin == useInteractive)
            {
                Console.Error.WriteLine("Specify exactly one of --interactive or --passcode-stdin.");
                return 2;
            }
            var passcode = useStdin ? await Console.In.ReadLineAsync() : PasscodeInput.ReadMasked();
            return await OperatorDatabase.ReplacePasscodeAsync(parseResult.GetValue(target) ?? string.Empty, passcode);
        });
        return command;
    }
}

using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class Program
{
    public static Task<int> Main(string[] args)
    {
        var root = new RootCommand("FaithTech Toronto AI Build Event database operator. Commands require an explicit target.");
        root.Subcommands.Add(VerifyConnectionCommand.Create());
        root.Subcommands.Add(SetAdminPasscodeCommand.Create());
        return root.Parse(args).InvokeAsync();
    }
}

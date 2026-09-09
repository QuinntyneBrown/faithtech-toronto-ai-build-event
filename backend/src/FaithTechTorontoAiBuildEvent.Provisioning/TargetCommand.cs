using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class TargetCommand : Command
{
    public TargetCommand() : base("target", "Configure and inspect explicit database targets.")
    {
        Subcommands.Add(new ConfigureTargetCommand());
        Subcommands.Add(new InspectTargetCommand());
    }
}

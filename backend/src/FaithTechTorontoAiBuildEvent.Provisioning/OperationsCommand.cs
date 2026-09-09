using System.CommandLine;
namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class OperationsCommand : Command
{
    public OperationsCommand() : base("operations", "Inspect, approve and reconcile reviewed operations.")
    {
        Subcommands.Add(new ApplyOperationCommand()); Subcommands.Add(new ReconcileOperationCommand()); Subcommands.Add(new ShowOperationCommand());
    }
}

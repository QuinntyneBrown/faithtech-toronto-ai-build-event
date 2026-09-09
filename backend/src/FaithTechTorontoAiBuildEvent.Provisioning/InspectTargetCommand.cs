using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class InspectTargetCommand : Command
{
    public InspectTargetCommand() : base("inspect", "Verify the resolved target and database principal without mutation.")
    {
        var options = new OperatorOptions(); options.AddTo(this);
        SetAction((parse, token) => OperatorCommandRunner.Run(parse, options, token, (_, _) => Task.FromResult<object?>(new {
            connectTimeoutSeconds = parse.GetValue(options.ConnectTimeout), commandTimeoutSeconds = parse.GetValue(options.CommandTimeout) })));
    }
}

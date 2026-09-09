using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class OperatorOptions
{
    public Option<string> Target { get; } = new("--target") { Required = true };
    public Option<string> Config { get; } = new("--config") { DefaultValueFactory = _ => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FaithTechTorontoAiBuildEvent/operator/targets.json") };
    public Option<bool> Json { get; } = new("--json");
    public Option<int> ConnectTimeout { get; } = new("--connect-timeout") { DefaultValueFactory = _ => 30 };
    public Option<int> CommandTimeout { get; } = new("--command-timeout") { DefaultValueFactory = _ => 60 };
    public void AddTo(Command command)
    {
        command.Options.Add(Target); command.Options.Add(Config); command.Options.Add(Json);
        command.Options.Add(ConnectTimeout); command.Options.Add(CommandTimeout);
    }
}

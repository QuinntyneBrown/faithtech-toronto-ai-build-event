using System.CommandLine;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class ConfigureTargetCommand : Command
{
    public ConfigureTargetCommand() : base("configure", "Save a target with a protected process-configuration reference, never credentials.")
    {
        var common = new OperatorOptions(); common.AddTo(this);
        var server = new Option<string>("--server") { Required = true };
        var database = new Option<string>("--database") { Required = true };
        var environment = new Option<string>("--environment") { Required = true };
        var connection = new Option<string>("--connection-env") { Required = true };
        Options.Add(server); Options.Add(database); Options.Add(environment); Options.Add(connection);
        SetAction(result => {
            try {
                new OperatorTargetProfileStore(result.GetValue(common.Config)!).Save(result.GetValue(common.Target)!,
                    new(result.GetValue(server)!, result.GetValue(database)!, result.GetValue(environment)!, result.GetValue(connection)!));
                Console.WriteLine("Target configured."); return 0;
            }
            catch (Exception error) when (error is ArgumentException or IOException or System.Text.Json.JsonException) {
                Console.Error.WriteLine("Target configuration failed; verify the absolute config path and profile."); return 2;
            }
        });
    }
}

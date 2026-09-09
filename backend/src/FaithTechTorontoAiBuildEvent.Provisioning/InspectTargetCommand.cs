using System.CommandLine;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Operations;
using Microsoft.Data.SqlClient;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class InspectTargetCommand : Command
{
    public InspectTargetCommand() : base("inspect", "Verify the resolved target and database principal without mutation.")
    {
        var options = new OperatorOptions(); options.AddTo(this);
        SetAction(async (result, token) => {
            try {
                var profile = new OperatorTargetProfileStore(result.GetValue(options.Config)!).Load(result.GetValue(options.Target)!);
                var secret = Environment.GetEnvironmentVariable(profile.ConnectionEnvironmentVariable ?? "") ?? throw new ArgumentException();
                await using var connection = new SqlConnection(SqlOperatorConnection.Configure(secret, profile.Server, profile.Database, profile.Environment, result.GetValue(options.ConnectTimeout)));
                await connection.OpenAsync(token);
                var principal = await SqlOperatorConnection.Identify(connection, profile.Environment, result.GetValue(options.CommandTimeout), token);
                Console.WriteLine(JsonSerializer.Serialize(new { target = new { principal.Server, principal.Database, principal.Environment }, principal }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                return 0;
            }
            catch (SqlException) { Console.Error.WriteLine("Target connection failed. Verify credentials, grants and network access."); return 3; }
            catch (Exception error) when (error is ArgumentException or InvalidOperationException or IOException or JsonException) {
                Console.Error.WriteLine("Invalid target configuration. No alternate connection was used."); return 2;
            }
        });
    }
}

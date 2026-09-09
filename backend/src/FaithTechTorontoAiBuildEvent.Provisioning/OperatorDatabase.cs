using Microsoft.Data.SqlClient;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class OperatorDatabase
{
    public static async Task<int> VerifyConnectionAsync(string target)
    {
        await using var connection = await OpenAsync(target);
        await using var command = new SqlCommand("SELECT @@SERVERNAME, DB_NAME();", connection);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        Console.WriteLine($"Connected to server {reader.GetString(0)}, database {reader.GetString(1)}.");
        return 0;
    }

    public static async Task<int> ReplacePasscodeAsync(string target, string? passcode)
    {
        if (passcode is null || passcode.Length != 4 || passcode.Any(character => character is < '0' or > '9'))
        {
            Console.Error.WriteLine("Passcode must contain exactly four ASCII digits.");
            return 2;
        }
        await using var connection = await OpenAsync(target);
        await using var command = new SqlCommand("dbo.ReplaceAdminPasscode", connection) { CommandType = System.Data.CommandType.StoredProcedure, CommandTimeout = 60 };
        command.Parameters.Add("@Passcode", System.Data.SqlDbType.NVarChar, -1).Value = passcode;
        await command.ExecuteNonQueryAsync();
        Console.WriteLine("Administrator passcode replaced.");
        return 0;
    }

    private static async Task<SqlConnection> OpenAsync(string target)
    {
        if (string.IsNullOrWhiteSpace(target)) throw new ArgumentException("--target is required.");
        var connectionString = Environment.GetEnvironmentVariable($"FAITHTECH_COMPANION_{target.ToUpperInvariant()}_CONNECTION")
            ?? throw new ArgumentException("No connection string exists for that explicit target.");
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (!builder.Encrypt || builder.TrustServerCertificate) throw new ArgumentException("The target must use encryption with certificate validation.");
        var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}

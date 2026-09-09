using Microsoft.Data.SqlClient;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class OperatorDatabase
{
    public static async Task<int> VerifyConnectionAsync(string target, int? connectionTimeoutSeconds, int? commandTimeoutSeconds)
    {
        try
        {
            var connectionTimeout = ResolveTimeout(connectionTimeoutSeconds, 30, "connection");
            var commandTimeout = ResolveTimeout(commandTimeoutSeconds, 60, "command");
            await using var connection = await OpenAsync(target, connectionTimeout);
            var (server, database) = await ReadIdentityAsync(connection, commandTimeout);
            VerifyExpectedIdentity(target, server, database);
            Console.WriteLine($"Verified target {target}: server {server}, database {database}.");
            return 0;
        }
        catch (ArgumentException exception) { return Report(2, exception.Message); }
        catch (SqlException exception) { return Report(1, $"Connection verification failed: {exception.Message}"); }
    }

    public static async Task<int> ReplacePasscodeAsync(string target, string? passcode, int? connectionTimeoutSeconds, int? commandTimeoutSeconds)
    {
        if (passcode is null || passcode.Length != 4 || passcode.Any(character => character is < '0' or > '9'))
        {
            Console.Error.WriteLine("Passcode must contain exactly four ASCII digits.");
            return 2;
        }
        try
        {
            var connectionTimeout = ResolveTimeout(connectionTimeoutSeconds, 30, "connection");
            var commandTimeout = ResolveTimeout(commandTimeoutSeconds, 60, "command");
            await using var connection = await OpenAsync(target, connectionTimeout);
            var (server, database) = await ReadIdentityAsync(connection, commandTimeout);
            VerifyExpectedIdentity(target, server, database);
            Console.WriteLine($"Verified target {target}: server {server}, database {database}.");
            await using var command = new SqlCommand("dbo.ReplaceAdminPasscode", connection) { CommandType = System.Data.CommandType.StoredProcedure, CommandTimeout = commandTimeout };
            command.Parameters.Add("@Passcode", System.Data.SqlDbType.NVarChar, -1).Value = passcode;
            try { await command.ExecuteNonQueryAsync(); }
            catch (SqlException exception) when (exception.Number == -2 || exception.Class >= 20)
            {
                return Report(3, "The passcode replacement result is unknown. It was not retried.");
            }
            catch (SqlException exception) { return Report(1, $"Passcode replacement failed: {exception.Message}"); }
            Console.WriteLine("Administrator passcode replaced.");
            return 0;
        }
        catch (ArgumentException exception) { return Report(2, exception.Message); }
        catch (SqlException exception) { return Report(1, $"Passcode replacement failed before execution: {exception.Message}"); }
    }

    private static async Task<SqlConnection> OpenAsync(string target, int connectionTimeout)
    {
        var prefix = TargetPrefix(target);
        var connectionString = Environment.GetEnvironmentVariable($"{prefix}_CONNECTION")
            ?? throw new ArgumentException("No connection string exists for that explicit target.");
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (!builder.Encrypt || builder.TrustServerCertificate) throw new ArgumentException("The target must use encryption with certificate validation.");
        builder.ConnectTimeout = connectionTimeout;
        var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static async Task<(string Server, string Database)> ReadIdentityAsync(SqlConnection connection, int commandTimeout)
    {
        await using var command = new SqlCommand("SELECT @@SERVERNAME, DB_NAME();", connection) { CommandTimeout = commandTimeout };
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return (reader.GetValue(0).ToString() ?? string.Empty, reader.GetValue(1).ToString() ?? string.Empty);
    }

    private static void VerifyExpectedIdentity(string target, string server, string database)
    {
        var prefix = TargetPrefix(target);
        var expectedServer = Environment.GetEnvironmentVariable($"{prefix}_SERVER")
            ?? throw new ArgumentException("The selected target has no expected server identity.");
        var expectedDatabase = Environment.GetEnvironmentVariable($"{prefix}_DATABASE")
            ?? throw new ArgumentException("The selected target has no expected database identity.");
        if (!string.Equals(server, expectedServer, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(database, expectedDatabase, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The connected server or database does not match the selected target.");
        }
    }

    private static string TargetPrefix(string target)
    {
        if (string.IsNullOrWhiteSpace(target) || target.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
        {
            throw new ArgumentException("--target must contain only letters, digits, or underscores.");
        }
        return $"FAITHTECH_COMPANION_{target.ToUpperInvariant()}";
    }

    private static int ResolveTimeout(int? requestedSeconds, int defaultSeconds, string kind)
    {
        if (requestedSeconds is null) return defaultSeconds;
        if (requestedSeconds <= 0) throw new ArgumentException($"The {kind} timeout must be a positive number of seconds.");
        return requestedSeconds.Value;
    }

    private static int Report(int exitCode, string message)
    {
        Console.Error.WriteLine(message);
        return exitCode;
    }
}

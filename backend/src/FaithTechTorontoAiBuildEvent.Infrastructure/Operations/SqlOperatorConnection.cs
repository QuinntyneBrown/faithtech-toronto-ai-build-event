using System.Security.Cryptography;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using Microsoft.Data.SqlClient;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

public static class SqlOperatorConnection
{
    public static string Configure(string secret, string server, string database, string environment, int connectTimeout = 30)
    {
        var value = new SqlConnectionStringBuilder(secret);
        if (connectTimeout <= 0 || value.MultipleActiveResultSets || value.AttachDBFilename.Length > 0 || value.FailoverPartner.Length > 0 ||
            Normalize(value.DataSource) != Normalize(server) || !value.InitialCatalog.Equals(database, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Connection does not match the selected target or uses unsupported options.");
        if (environment.Equals("production", StringComparison.OrdinalIgnoreCase) &&
            (value.Encrypt == SqlConnectionEncryptOption.Optional || value.TrustServerCertificate))
            throw new ArgumentException("Production requires encryption and certificate validation.");
        value.ConnectTimeout = connectTimeout;
        value.ApplicationName = "FaithTech operator";
        return value.ConnectionString;
    }

    public static async Task<OperatorPrincipal> Identify(SqlConnection connection, string environment, int timeout, CancellationToken token)
    {
        if (timeout <= 0) throw new ArgumentException("Command timeout must be positive.");
        await using var command = connection.CreateCommand();
        command.CommandTimeout = timeout;
        command.CommandText = "SELECT CONVERT(nvarchar(256), SERVERPROPERTY('ServerName')), DB_NAME(), USER_NAME(), ORIGINAL_LOGIN(), SUSER_SID(), sid FROM sys.database_principals WHERE principal_id = USER_ID()";
        await using var reader = await command.ExecuteReaderAsync(token);
        if (!await reader.ReadAsync(token) || Enumerable.Range(0, 6).Any(reader.IsDBNull))
            throw new InvalidOperationException("Database identity unavailable.");
        var server = reader.GetString(0); var database = reader.GetString(1);
        var key = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new[] {
            Normalize(server), database.ToUpperInvariant(), Convert.ToHexString((byte[])reader[4]), Convert.ToHexString((byte[])reader[5]) })));
        return new(server, database, environment, key, reader.GetString(2), reader.GetString(3));
    }

    public static string Normalize(string value)
    {
        value = value.Trim().ToLowerInvariant();
        if (value.StartsWith("tcp:", StringComparison.Ordinal)) value = value[4..];
        if (value.EndsWith(",1433", StringComparison.Ordinal)) value = value[..^5];
        return value;
    }
}

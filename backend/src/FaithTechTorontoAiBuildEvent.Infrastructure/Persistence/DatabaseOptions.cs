using Microsoft.Data.SqlClient;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public string EventDatabase { get; set; } = "";

    public bool IsValid()
    {
        try
        {
            var connection = new SqlConnectionStringBuilder(EventDatabase);
            return !string.IsNullOrWhiteSpace(connection.DataSource) &&
                !string.IsNullOrWhiteSpace(connection.InitialCatalog) && !connection.MultipleActiveResultSets;
        }
        catch (ArgumentException) { return false; }
    }
}

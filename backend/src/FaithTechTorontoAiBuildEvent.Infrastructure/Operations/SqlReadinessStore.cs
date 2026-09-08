using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
namespace FaithTechTorontoAiBuildEvent.Infrastructure.Operations;
public sealed class SqlReadinessStore(EventDbContext database) : IReadinessStore
{
    public async Task<bool> IsReady(CancellationToken cancellationToken)
    {
        try
        {
            return await database.Database.CanConnectAsync(cancellationToken) &&
                !(await database.Database.GetPendingMigrationsAsync(cancellationToken)).Any();
        }
        catch (SqlException) { return false; }
    }
}

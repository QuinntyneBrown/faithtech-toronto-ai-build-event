using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

public sealed class SqlReadinessStore(CompanionDbContext database) : IReadinessStore
{
    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await database.EventStates.AnyAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}

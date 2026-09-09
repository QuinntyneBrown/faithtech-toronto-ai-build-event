using FaithTechTorontoAiBuildEvent.Application.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.EventFlow;

public sealed class SqlEventFlowStore(CompanionDbContext database) : IEventFlowStore
{
    public async Task<bool> AdvanceAsync(long expectedVersion, string fromScreen, string toScreen, CancellationToken cancellationToken)
    {
        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.Version != expectedVersion || !Enum.TryParse<EventScreen>(fromScreen, true, out var from) || !Enum.TryParse<EventScreen>(toScreen, true, out var to))
        {
            return false;
        }
        if (state.CurrentScreen != from || (from, to) is not (EventScreen.Countdown, EventScreen.Projects) and not (EventScreen.Projects, EventScreen.Teams) and not (EventScreen.Teams, EventScreen.Raffle))
        {
            return false;
        }

        state.CurrentScreen = to;
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }
}

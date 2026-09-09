using FaithTechTorontoAiBuildEvent.Application.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FaithTechTorontoAiBuildEvent.Domain.Teams;
using System.Security.Cryptography;
using FaithTechTorontoAiBuildEvent.Application.EventState;
using FaithTechTorontoAiBuildEvent.Domain.Operations;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.EventFlow;

public sealed class SqlEventFlowStore(CompanionDbContext database, IEventUpdatePublisher updatePublisher) : IEventFlowStore
{
    public async Task<bool> AdvanceAsync(Guid operationId, byte[] inputDigest, long expectedVersion, string fromScreen, string toScreen, CancellationToken cancellationToken)
    {
        var receipt = await database.EventOperationReceipts.SingleOrDefaultAsync(candidate => candidate.OperationId == operationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.OperationKind != "advance-screen" || !CryptographicOperations.FixedTimeEquals(receipt.InputDigest, inputDigest))
            {
                throw new ArgumentException("The operation identity was already used with different input.");
            }
            return true;
        }

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
        if (from == EventScreen.Projects && to == EventScreen.Teams && !state.TeamsFormed)
        {
            var participants = await database.Participants.OrderBy(participant => participant.Id).ToListAsync(cancellationToken);
            Shuffle(participants);
            for (var index = 0; index < participants.Count; index += 3)
            {
                var team = new Team { Id = Guid.NewGuid(), Label = $"Team {state.NextTeamLabel:D2}" };
                state.NextTeamLabel++;
                database.Teams.Add(team);
                foreach (var participant in participants.Skip(index).Take(3))
                {
                    participant.TeamId = team.Id;
                }
            }
            state.TeamsFormed = true;
        }
        state.Version++;
        database.EventOperationReceipts.Add(new EventOperationReceipt
        {
            OperationId = operationId,
            OperationKind = "advance-screen",
            InputDigest = inputDigest,
            ResultVersion = state.Version,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
        return true;
    }

    private static void Shuffle<T>(IList<T> values)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var other = RandomNumberGenerator.GetInt32(index + 1);
            (values[index], values[other]) = (values[other], values[index]);
        }
    }
}

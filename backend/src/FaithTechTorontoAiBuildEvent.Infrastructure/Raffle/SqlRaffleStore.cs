using System.Security.Cryptography;
using FaithTechTorontoAiBuildEvent.Application.Raffle;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.Raffle;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Raffle;

public sealed class SqlRaffleStore(CompanionDbContext database) : IRaffleStore
{
    public async Task<DrawWinnerResult> DrawAsync(long expectedVersion, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.CurrentScreen != EventScreen.Raffle || state.Version != expectedVersion)
        {
            throw new InvalidOperationException("Raffle state changed; reload and try again.");
        }

        var priorWinners = await database.RaffleDraws.Select(draw => draw.WinnerParticipantId).ToListAsync(cancellationToken);
        var eligibleParticipants = await database.Participants
            .Where(participant => !priorWinners.Contains(participant.Id))
            .OrderBy(participant => participant.Id)
            .ToListAsync(cancellationToken);
        if (eligibleParticipants.Count == 0)
        {
            throw new InvalidOperationException("No eligible raffle participants remain.");
        }

        var winner = eligibleParticipants[RandomNumberGenerator.GetInt32(eligibleParticipants.Count)];
        var draw = new RaffleDraw
        {
            Id = Guid.NewGuid(),
            WinnerParticipantId = winner.Id,
            WinnerLabel = winner.Name ?? winner.PublicLabel,
            StartedAtUtc = nowUtc,
            RevealAtUtc = nowUtc.AddSeconds(5),
            EffectsEndAtUtc = nowUtc.AddSeconds(10)
        };
        database.RaffleDraws.Add(draw);
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        return new DrawWinnerResult(draw.Id, draw.WinnerLabel, draw.RevealAtUtc, draw.EffectsEndAtUtc);
    }
}

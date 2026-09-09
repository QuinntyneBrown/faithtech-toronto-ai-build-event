using System.Security.Cryptography;
using FaithTechTorontoAiBuildEvent.Application.Raffle;
using FaithTechTorontoAiBuildEvent.Application.EventState;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.Participants;
using FaithTechTorontoAiBuildEvent.Domain.Raffle;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Raffle;

public sealed class SqlRaffleStore(CompanionDbContext database, IEventUpdatePublisher updatePublisher) : IRaffleStore
{
    public async Task<RaffleSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var winners = await database.RaffleDraws.AsNoTracking()
            .OrderByDescending(draw => draw.StartedAtUtc)
            .ToListAsync(cancellationToken);
        var candidates = await database.RaffleCandidates.AsNoTracking().ToListAsync(cancellationToken);
        var candidateLabels = candidates.GroupBy(candidate => candidate.DrawId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.OrderBy(candidate => candidate.Label).Select(candidate => candidate.Label).ToList());
        var results = winners.Select(draw => ToResult(draw, candidateLabels)).ToList();
        var winnerIds = await database.RaffleDraws.AsNoTracking().Select(draw => draw.WinnerParticipantId).ToListAsync(cancellationToken);
        var eligibleCount = await database.Participants.CountAsync(participant => !winnerIds.Contains(participant.Id), cancellationToken);
        return new RaffleSnapshot(eligibleCount, results.FirstOrDefault(), results);
    }

    public async Task<DrawWinnerResult> DrawAsync(Guid operationId, long expectedVersion, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var existing = await database.RaffleDraws.SingleOrDefaultAsync(draw => draw.OperationId == operationId, cancellationToken);
        if (existing is not null)
        {
            if (existing.ExpectedVersion != expectedVersion)
            {
                throw new InvalidOperationException("This operation identity was already used with different input.");
            }
            return new DrawWinnerResult(existing.Id, existing.WinnerLabel, existing.RevealAtUtc, existing.EffectsEndAtUtc);
        }

        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.CurrentScreen != EventScreen.Raffle || state.Version != expectedVersion)
        {
            throw new InvalidOperationException("Raffle state changed; reload and try again.");
        }

        if (await database.RaffleDraws.AnyAsync(draw => draw.EffectsEndAtUtc > nowUtc, cancellationToken))
        {
            throw new InvalidOperationException("The current raffle draw is still presenting.");
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
            OperationId = operationId,
            ExpectedVersion = expectedVersion,
            WinnerParticipantId = winner.Id,
            WinnerLabel = ParticipantPublicDisplay.Format(winner.Name, winner.PublicLabel),
            StartedAtUtc = nowUtc,
            RevealAtUtc = nowUtc.AddSeconds(5),
            EffectsEndAtUtc = nowUtc.AddSeconds(10)
        };
        database.RaffleDraws.Add(draw);
        database.RaffleCandidates.AddRange(eligibleParticipants.Select(participant => new RaffleCandidate
        {
            DrawId = draw.Id,
            ParticipantId = participant.Id,
            Label = ParticipantPublicDisplay.Format(participant.Name, participant.PublicLabel)
        }));
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
        return new DrawWinnerResult(draw.Id, draw.WinnerLabel, draw.RevealAtUtc, draw.EffectsEndAtUtc);
    }

    private static RaffleResult ToResult(RaffleDraw draw, IReadOnlyDictionary<Guid, IReadOnlyList<string>> candidateLabels)
        => new(draw.Id, draw.WinnerLabel, candidateLabels.GetValueOrDefault(draw.Id, []), draw.StartedAtUtc, draw.RevealAtUtc, draw.EffectsEndAtUtc);
}

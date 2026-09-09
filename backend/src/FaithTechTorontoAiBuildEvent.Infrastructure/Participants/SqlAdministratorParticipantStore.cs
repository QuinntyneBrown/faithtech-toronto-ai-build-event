using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FaithTechTorontoAiBuildEvent.Domain.Participants;
using FaithTechTorontoAiBuildEvent.Application.EventState;
using FaithTechTorontoAiBuildEvent.Domain.Operations;
using System.Security.Cryptography;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class SqlAdministratorParticipantStore(CompanionDbContext database, IEventUpdatePublisher updatePublisher) : IAdministratorParticipantStore
{
    public async Task<AdministratorParticipant> AddAsync(Guid operationId, byte[] inputDigest, string email, string normalizedEmail, long expectedVersion, CancellationToken cancellationToken)
    {
        var receipt = await database.EventOperationReceipts.SingleOrDefaultAsync(candidate => candidate.OperationId == operationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.OperationKind != "add-administrator-participant"
                || !CryptographicOperations.FixedTimeEquals(receipt.InputDigest, inputDigest)
                || receipt.ResultJson is null)
            {
                throw new EntryValidationException("The operation identity was already used with different input.");
            }
            return JsonSerializer.Deserialize<AdministratorParticipant>(receipt.ResultJson)
                ?? throw new InvalidOperationException("The saved operation result is unavailable.");
        }

        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.Version != expectedVersion)
        {
            throw new InvalidOperationException("Participant roster changed; reload and try again.");
        }
        if (await database.Participants.AnyAsync(participant => participant.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            throw new EntryValidationException("This email is already entered.");
        }
        var participant = new Participant { Id = Guid.NewGuid(), Email = email, NormalizedEmail = normalizedEmail, PublicLabel = $"Participant {state.NextParticipantLabel:D3}" };
        state.NextParticipantLabel++;
        state.Version++;
        database.Participants.Add(participant);
        var result = new AdministratorParticipant(participant.Id, participant.Email, participant.PublicLabel, null, null, null, null, false);
        database.EventOperationReceipts.Add(new EventOperationReceipt
        {
            OperationId = operationId,
            OperationKind = "add-administrator-participant",
            InputDigest = inputDigest,
            ResultId = participant.Id,
            ResultJson = JsonSerializer.Serialize(result),
            ResultVersion = state.Version,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
        return result;
    }

    public async Task DeleteAsync(Guid participantId, long expectedVersion, CancellationToken cancellationToken)
    {
        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.Version != expectedVersion)
        {
            throw new InvalidOperationException("Participant roster changed; reload and try again.");
        }
        var participant = await database.Participants.SingleOrDefaultAsync(candidate => candidate.Id == participantId, cancellationToken) ?? throw new KeyNotFoundException("Participant not found.");
        var sessions = await database.ParticipantSessions.Where(session => session.ParticipantId == participantId).ToListAsync(cancellationToken);
        foreach (var session in sessions) { session.Revoked = true; }
        var receipts = await database.EntryReceipts.Where(receipt => receipt.ParticipantId == participantId).ToListAsync(cancellationToken);
        foreach (var receipt in receipts) { receipt.Revoked = true; }
        var profileReceipts = await database.ProfileSaveReceipts.Where(receipt => receipt.ParticipantId == participantId).ToListAsync(cancellationToken);
        database.ProfileSaveReceipts.RemoveRange(profileReceipts);
        var draws = await database.RaffleDraws.Where(draw => draw.WinnerParticipantId == participantId).ToListAsync(cancellationToken);
        foreach (var draw in draws) { draw.WinnerParticipantId = null; draw.WinnerLabel = "Removed participant"; }
        var candidates = await database.RaffleCandidates.Where(candidate => candidate.ParticipantId == participantId).ToListAsync(cancellationToken);
        foreach (var candidate in candidates) { candidate.Label = "Removed participant"; }
        database.Participants.Remove(participant);
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
    }

    public async Task<AdministratorParticipant> UpdateAsync(Guid operationId, byte[] inputDigest, Guid participantId, string email, string normalizedEmail, AdministratorParticipantInput input, long expectedVersion, CancellationToken cancellationToken)
    {
        var receipt = await database.EventOperationReceipts.SingleOrDefaultAsync(candidate => candidate.OperationId == operationId, cancellationToken);
        if (receipt is not null)
        {
            if (receipt.OperationKind != "update-administrator-participant"
                || !CryptographicOperations.FixedTimeEquals(receipt.InputDigest, inputDigest)
                || receipt.ResultJson is null)
            {
                throw new EntryValidationException("The operation identity was already used with different input.");
            }
            return JsonSerializer.Deserialize<AdministratorParticipant>(receipt.ResultJson)
                ?? throw new InvalidOperationException("The saved operation result is unavailable.");
        }

        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.Version != expectedVersion) throw new InvalidOperationException("Participant roster changed; reload and try again.");
        var participant = await database.Participants.SingleOrDefaultAsync(candidate => candidate.Id == participantId, cancellationToken) ?? throw new KeyNotFoundException("Participant not found.");
        if (await database.Participants.AnyAsync(candidate => candidate.Id != participantId && candidate.NormalizedEmail == normalizedEmail, cancellationToken)) throw new EntryValidationException("This email is already entered.");
        participant.Email = email;
        participant.NormalizedEmail = normalizedEmail;
        participant.Name = BlankToNull(input.Name);
        participant.WhatYouMake = BlankToNull(input.WhatYouMake);
        participant.OnYourHeart = BlankToNull(input.OnYourHeart);
        state.Version++;
        var teamLabel = participant.TeamId is { } teamId ? await database.Teams.Where(team => team.Id == teamId).Select(team => team.Label).SingleOrDefaultAsync(cancellationToken) : null;
        var hasWon = await database.RaffleDraws.AnyAsync(draw => draw.WinnerParticipantId == participantId, cancellationToken);
        var result = new AdministratorParticipant(participant.Id, participant.Email, participant.PublicLabel, participant.Name, participant.WhatYouMake, participant.OnYourHeart, teamLabel, hasWon);
        database.EventOperationReceipts.Add(new EventOperationReceipt
        {
            OperationId = operationId,
            OperationKind = "update-administrator-participant",
            InputDigest = inputDigest,
            ResultId = participant.Id,
            ResultJson = JsonSerializer.Serialize(result),
            ResultVersion = state.Version,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<AdministratorParticipant>> ListAsync(CancellationToken cancellationToken)
    {
        var participants = await database.Participants.AsNoTracking().OrderBy(participant => participant.PublicLabel).ToListAsync(cancellationToken);
        var teams = await database.Teams.AsNoTracking().ToDictionaryAsync(team => team.Id, team => team.Label, cancellationToken);
        var winners = await database.RaffleDraws.AsNoTracking().Select(draw => draw.WinnerParticipantId).ToListAsync(cancellationToken);
        return participants.Select(participant => new AdministratorParticipant(participant.Id, participant.Email, participant.PublicLabel, participant.Name, participant.WhatYouMake, participant.OnYourHeart, participant.TeamId is { } teamId && teams.TryGetValue(teamId, out var label) ? label : null, winners.Contains(participant.Id))).ToList();
    }

    private static string? BlankToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().Replace("\r\n", "\n", StringComparison.Ordinal);
}

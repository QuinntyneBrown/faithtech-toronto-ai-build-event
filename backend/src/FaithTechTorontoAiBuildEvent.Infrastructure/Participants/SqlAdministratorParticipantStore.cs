using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class SqlAdministratorParticipantStore(CompanionDbContext database) : IAdministratorParticipantStore
{
    public async Task<IReadOnlyList<AdministratorParticipant>> ListAsync(CancellationToken cancellationToken)
    {
        var participants = await database.Participants.AsNoTracking().OrderBy(participant => participant.PublicLabel).ToListAsync(cancellationToken);
        var teams = await database.Teams.AsNoTracking().ToDictionaryAsync(team => team.Id, team => team.Label, cancellationToken);
        var winners = await database.RaffleDraws.AsNoTracking().Select(draw => draw.WinnerParticipantId).ToListAsync(cancellationToken);
        return participants.Select(participant => new AdministratorParticipant(participant.Id, participant.Email, participant.PublicLabel, participant.Name, participant.WhatYouMake, participant.OnYourHeart, participant.TeamId is { } teamId && teams.TryGetValue(teamId, out var label) ? label : null, winners.Contains(participant.Id))).ToList();
    }
}

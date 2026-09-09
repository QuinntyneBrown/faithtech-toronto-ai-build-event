using FaithTechTorontoAiBuildEvent.Application.Teams;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.Teams;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainEventState = FaithTechTorontoAiBuildEvent.Domain.EventFlow.EventState;
using FaithTechTorontoAiBuildEvent.Application.EventState;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Teams;

public sealed class SqlTeamStore(CompanionDbContext database, IEventUpdatePublisher updatePublisher) : ITeamStore
{
    public async Task AssignProjectAsync(Guid teamId, Guid? projectId, long expectedVersion, CancellationToken cancellationToken)
    {
        var state = await GetTeamsStateAsync(expectedVersion, cancellationToken);
        var team = await database.Teams.SingleOrDefaultAsync(candidate => candidate.Id == teamId, cancellationToken) ?? throw new KeyNotFoundException("Team not found.");
        if (projectId is not null && !await database.Projects.AnyAsync(project => project.Id == projectId, cancellationToken))
        {
            throw new KeyNotFoundException("Project not found.");
        }
        team.ProjectId = projectId;
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
    }

    public async Task MoveAsync(Guid participantId, string destination, Guid? teamId, long expectedVersion, CancellationToken cancellationToken)
    {
        var state = await GetTeamsStateAsync(expectedVersion, cancellationToken);
        var participant = await database.Participants.SingleOrDefaultAsync(candidate => candidate.Id == participantId, cancellationToken)
            ?? throw new KeyNotFoundException("Participant not found.");

        switch (destination.ToLowerInvariant())
        {
            case "unassigned" when teamId is null:
                participant.TeamId = null;
                break;
            case "new" when teamId is null:
                var newTeam = new Team { Id = Guid.NewGuid(), Label = $"Team {state.NextTeamLabel:D2}" };
                state.NextTeamLabel++;
                database.Teams.Add(newTeam);
                participant.TeamId = newTeam.Id;
                break;
            case "existing" when teamId is not null && await database.Teams.AnyAsync(team => team.Id == teamId, cancellationToken):
                participant.TeamId = teamId;
                break;
            default:
                throw new ArgumentException("Choose an existing team, New team, or Unassigned.");
        }

        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
    }

    private async Task<DomainEventState> GetTeamsStateAsync(long expectedVersion, CancellationToken cancellationToken)
    {
        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.CurrentScreen != EventScreen.Teams || state.Version != expectedVersion || !state.TeamsFormed)
        {
            throw new InvalidOperationException("Team selection changed; reload and try again.");
        }
        return state;
    }
}

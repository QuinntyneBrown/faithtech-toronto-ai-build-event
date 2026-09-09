using FaithTechTorontoAiBuildEvent.Application.EventState;
using FaithTechTorontoAiBuildEvent.Application.Raffle;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.EventState;

public sealed class SqlEventStateStore(CompanionDbContext database, IOptions<EventOptions> options) : IEventStateStore
{
    public async Task<PublicEventSnapshot> GetPublicSnapshotAsync(CancellationToken cancellationToken)
    {
        var state = await database.EventStates.AsNoTracking().SingleAsync(cancellationToken);
        var projects = await database.Projects.AsNoTracking()
            .OrderBy(project => project.Title)
            .Select(project => new ProjectCard(project.Id, project.Title, project.Description, project.RepositoryUrl, project.DemoUrl))
            .ToListAsync(cancellationToken);
        var teams = await database.Teams.AsNoTracking().OrderBy(team => team.Label).ToListAsync(cancellationToken);
        var participants = await database.Participants.AsNoTracking().ToListAsync(cancellationToken);
        var draws = await database.RaffleDraws.AsNoTracking().OrderByDescending(draw => draw.StartedAtUtc).ToListAsync(cancellationToken);
        var candidates = await database.RaffleCandidates.AsNoTracking().ToListAsync(cancellationToken);
        var candidateLabels = candidates.GroupBy(candidate => candidate.DrawId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.OrderBy(candidate => candidate.Label).Select(candidate => candidate.Label).ToList());
        var publicTeams = teams.Select(team => new PublicTeam(
            team.Id,
            team.Label,
            team.ProjectId,
            participants.Where(participant => participant.TeamId == team.Id).OrderBy(participant => participant.PublicLabel).Select(participant => participant.Name ?? participant.PublicLabel).ToList()))
            .ToList();
        var eventOptions = options.Value;
        var publicDraws = draws.Select(draw => new RaffleResult(draw.Id, draw.WinnerLabel, candidateLabels.GetValueOrDefault(draw.Id, []), draw.StartedAtUtc, draw.RevealAtUtc, draw.EffectsEndAtUtc)).ToList();
        var raffle = new RaffleSnapshot(participants.Count(participant => draws.All(draw => draw.WinnerParticipantId != participant.Id)), publicDraws.FirstOrDefault(), publicDraws);

        return new PublicEventSnapshot(
            state.Version.ToString(System.Globalization.CultureInfo.InvariantCulture),
            state.CurrentScreen.ToString().ToLowerInvariant(),
            eventOptions.Title,
            eventOptions.Welcome,
            eventOptions.Purpose,
            eventOptions.Venue,
            eventOptions.EventTime,
            eventOptions.CountdownTargetUtc,
            projects,
            publicTeams,
            raffle);
    }

    public Task<DateTimeOffset> GetServerTimeAsync(CancellationToken cancellationToken)
        => Task.FromResult(DateTimeOffset.UtcNow);

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

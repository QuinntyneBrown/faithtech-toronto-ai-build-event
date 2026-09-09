using FaithTechTorontoAiBuildEvent.Application.EventState;
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
        var eventOptions = options.Value;

        return new PublicEventSnapshot(
            state.Version.ToString(System.Globalization.CultureInfo.InvariantCulture),
            state.CurrentScreen.ToString().ToLowerInvariant(),
            eventOptions.Title,
            eventOptions.Welcome,
            eventOptions.Purpose,
            eventOptions.Venue,
            eventOptions.EventTime,
            eventOptions.CountdownTargetUtc,
            projects);
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

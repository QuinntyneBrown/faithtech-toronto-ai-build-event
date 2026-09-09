using FaithTechTorontoAiBuildEvent.Application.Projects;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.Projects;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainEventState = FaithTechTorontoAiBuildEvent.Domain.EventFlow.EventState;
using FaithTechTorontoAiBuildEvent.Application.EventState;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Projects;

public sealed class SqlProjectStore(CompanionDbContext database, IEventUpdatePublisher updatePublisher) : IProjectStore
{
    public async Task UpdateAsync(Guid projectId, long expectedVersion, ProjectInput input, CancellationToken cancellationToken)
    {
        var state = await GetLiveStateAsync(expectedVersion, cancellationToken);
        var project = await database.Projects.SingleOrDefaultAsync(project => project.Id == projectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");
        project.Title = input.Title.Trim();
        project.Description = input.Description.Trim();
        project.RepositoryUrl = BlankToNull(input.RepositoryUrl);
        project.DemoUrl = BlankToNull(input.DemoUrl);
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
    }

    public async Task DeleteAsync(Guid projectId, long expectedVersion, CancellationToken cancellationToken)
    {
        var state = await GetLiveStateAsync(expectedVersion, cancellationToken);
        var project = await database.Projects.SingleOrDefaultAsync(project => project.Id == projectId, cancellationToken)
            ?? throw new KeyNotFoundException("Project not found.");
        var teams = await database.Teams.Where(team => team.ProjectId == projectId).ToListAsync(cancellationToken);
        foreach (var team in teams)
        {
            team.ProjectId = null;
        }
        database.Projects.Remove(project);
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
    }

    public async Task<Guid> AddAsync(long expectedVersion, ProjectInput input, CancellationToken cancellationToken)
    {
        var state = await GetLiveStateAsync(expectedVersion, cancellationToken);
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            RepositoryUrl = BlankToNull(input.RepositoryUrl),
            DemoUrl = BlankToNull(input.DemoUrl)
        };
        database.Projects.Add(project);
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
        return project.Id;
    }

    private static string? BlankToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<DomainEventState> GetLiveStateAsync(long expectedVersion, CancellationToken cancellationToken)
    {
        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.CurrentScreen != EventScreen.Projects || state.Version != expectedVersion)
        {
            throw new InvalidOperationException("Project catalogue changed; reload and try again.");
        }
        return state;
    }
}

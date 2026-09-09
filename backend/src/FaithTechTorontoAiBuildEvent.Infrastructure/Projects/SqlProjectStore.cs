using FaithTechTorontoAiBuildEvent.Application.Projects;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.Projects;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Projects;

public sealed class SqlProjectStore(CompanionDbContext database) : IProjectStore
{
    public async Task<Guid> AddAsync(long expectedVersion, ProjectInput input, CancellationToken cancellationToken)
    {
        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.CurrentScreen != EventScreen.Projects || state.Version != expectedVersion)
        {
            throw new InvalidOperationException("Project catalogue changed; reload and try again.");
        }
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
        return project.Id;
    }

    private static string? BlankToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

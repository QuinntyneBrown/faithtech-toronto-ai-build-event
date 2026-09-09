namespace FaithTechTorontoAiBuildEvent.Application.Projects;

public interface IProjectStore
{
    Task<Guid> AddAsync(long expectedVersion, ProjectInput input, CancellationToken cancellationToken);
    Task UpdateAsync(Guid projectId, long expectedVersion, ProjectInput input, CancellationToken cancellationToken);
    Task DeleteAsync(Guid projectId, long expectedVersion, CancellationToken cancellationToken);
}

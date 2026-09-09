namespace FaithTechTorontoAiBuildEvent.Application.Projects;

public interface IProjectStore
{
    Task<Guid> AddAsync(long expectedVersion, ProjectInput input, CancellationToken cancellationToken);
}

namespace FaithTechTorontoAiBuildEvent.Application.Projects;

public interface IProjectStore
{
    Task<Guid> AddAsync(Guid operationId, byte[] inputDigest, long expectedVersion, ProjectInput input, CancellationToken cancellationToken);
    Task UpdateAsync(Guid operationId, byte[] inputDigest, Guid projectId, long expectedVersion, ProjectInput input, CancellationToken cancellationToken);
    Task DeleteAsync(Guid projectId, long expectedVersion, CancellationToken cancellationToken);
}

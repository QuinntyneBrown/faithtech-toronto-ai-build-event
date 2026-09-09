using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Projects;

public sealed record SaveProjectCommand(Guid OperationId, string ExpectedVersion, ProjectInput Input, string? AdministratorSecret) : IRequest<Guid>;

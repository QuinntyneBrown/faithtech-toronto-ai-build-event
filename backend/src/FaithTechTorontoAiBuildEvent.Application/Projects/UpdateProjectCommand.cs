using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Projects;

public sealed record UpdateProjectCommand(Guid OperationId, Guid ProjectId, string ExpectedVersion, ProjectInput Input, string? AdministratorSecret) : IRequest;

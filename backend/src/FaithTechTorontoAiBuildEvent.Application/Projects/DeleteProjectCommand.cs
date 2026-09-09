using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Projects;

public sealed record DeleteProjectCommand(Guid OperationId, Guid ProjectId, string ExpectedVersion, string? AdministratorSecret) : IRequest;

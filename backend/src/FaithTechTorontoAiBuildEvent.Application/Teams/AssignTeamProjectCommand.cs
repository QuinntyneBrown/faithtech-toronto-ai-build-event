using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Teams;

public sealed record AssignTeamProjectCommand(Guid OperationId, Guid TeamId, Guid? ProjectId, string ExpectedVersion, string? AdministratorSecret) : IRequest;

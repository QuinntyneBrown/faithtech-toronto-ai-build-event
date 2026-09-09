using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Teams;

public sealed record MoveTeamMemberCommand(Guid OperationId, Guid ParticipantId, string Destination, Guid? TeamId, string ExpectedVersion, string? AdministratorSecret) : IRequest;

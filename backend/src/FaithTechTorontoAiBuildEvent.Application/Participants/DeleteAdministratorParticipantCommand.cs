using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record DeleteAdministratorParticipantCommand(Guid OperationId, Guid ParticipantId, string ExpectedVersion, string? AdministratorSecret) : IRequest;

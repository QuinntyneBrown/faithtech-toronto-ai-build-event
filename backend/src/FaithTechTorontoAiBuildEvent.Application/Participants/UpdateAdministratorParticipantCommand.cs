using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record UpdateAdministratorParticipantCommand(Guid OperationId, Guid ParticipantId, string ExpectedVersion, AdministratorParticipantInput Input, string? AdministratorSecret) : IRequest<AdministratorParticipant>;

using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record AddAdministratorParticipantCommand(Guid OperationId, string ExpectedVersion, string Email, string? AdministratorSecret) : IRequest<AdministratorParticipant>;

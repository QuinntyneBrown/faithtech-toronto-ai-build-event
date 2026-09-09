using FaithTechTorontoAiBuildEvent.Application.Participants;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record UpdateAdministratorParticipantRequest(Guid OperationId, string ExpectedVersion, AdministratorParticipantInput Input);

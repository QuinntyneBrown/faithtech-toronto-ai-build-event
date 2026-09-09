namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record AddAdministratorParticipantRequest(Guid OperationId, string ExpectedVersion, string Email);

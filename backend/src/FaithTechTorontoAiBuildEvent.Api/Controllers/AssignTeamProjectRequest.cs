namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record AssignTeamProjectRequest(Guid OperationId, string ExpectedVersion, Guid? ProjectId);

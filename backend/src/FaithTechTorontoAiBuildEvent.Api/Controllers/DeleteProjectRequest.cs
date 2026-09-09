namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record DeleteProjectRequest(Guid OperationId, string ExpectedVersion);

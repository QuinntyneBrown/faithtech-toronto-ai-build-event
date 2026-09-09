using FaithTechTorontoAiBuildEvent.Application.Projects;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record SaveProjectRequest(Guid OperationId, string ExpectedVersion, ProjectInput Input);

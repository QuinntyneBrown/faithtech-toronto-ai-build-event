namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record AdvanceScreenRequest(Guid OperationId, string ExpectedVersion, string FromScreen, string ToScreen);

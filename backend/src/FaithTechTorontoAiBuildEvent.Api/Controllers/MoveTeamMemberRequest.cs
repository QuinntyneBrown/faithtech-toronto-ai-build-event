namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record MoveTeamMemberRequest(Guid OperationId, string ExpectedVersion, Guid ParticipantId, string Destination, Guid? TeamId);

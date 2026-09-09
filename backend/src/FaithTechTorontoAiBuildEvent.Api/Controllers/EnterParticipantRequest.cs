using FaithTechTorontoAiBuildEvent.Application.Participants;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record EnterParticipantRequest(Guid OperationId, string ExpectedVersion, EnterParticipantInput Input);

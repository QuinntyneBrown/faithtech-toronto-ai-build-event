using FaithTechTorontoAiBuildEvent.Application.Participants;

namespace FaithTechTorontoAiBuildEvent.Api.Controllers;

public sealed record SaveParticipantProfileRequest(Guid OperationId, string ExpectedVersion, ProfileInput Input);

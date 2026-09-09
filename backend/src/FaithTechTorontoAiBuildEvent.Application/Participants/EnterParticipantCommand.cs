using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record EnterParticipantCommand(Guid OperationId, string ExpectedVersion, EnterParticipantInput Input, string? ReceiptSecret)
    : IRequest<EntryResult>;

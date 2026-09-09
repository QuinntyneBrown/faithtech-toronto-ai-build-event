using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record CreateEntryReceiptCommand(string? ExistingSecret) : IRequest<EntryReceiptBootstrap>;

using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.EventFlow;

public sealed record AdvanceScreenCommand(Guid OperationId, string ExpectedVersion, string FromScreen, string ToScreen, string? AdministratorSecret) : IRequest<bool>;

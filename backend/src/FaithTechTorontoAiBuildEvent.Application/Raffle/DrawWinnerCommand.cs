using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public sealed record DrawWinnerCommand(Guid OperationId, string ExpectedVersion, string? AdministratorSecret) : IRequest<DrawWinnerResult>;

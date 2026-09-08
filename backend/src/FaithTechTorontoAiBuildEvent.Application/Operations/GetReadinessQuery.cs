using MediatR;
namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public sealed record GetReadinessQuery : IRequest<ReadinessState>;

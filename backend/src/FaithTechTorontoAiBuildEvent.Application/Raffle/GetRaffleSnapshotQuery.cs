using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public sealed record GetRaffleSnapshotQuery : IRequest<RaffleSnapshot>;

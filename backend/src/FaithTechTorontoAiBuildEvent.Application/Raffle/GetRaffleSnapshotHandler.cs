using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public sealed class GetRaffleSnapshotHandler(IRaffleStore raffleStore) : IRequestHandler<GetRaffleSnapshotQuery, RaffleSnapshot>
{
    public Task<RaffleSnapshot> Handle(GetRaffleSnapshotQuery request, CancellationToken cancellationToken)
        => raffleStore.GetSnapshotAsync(cancellationToken);
}

using FaithTechTorontoAiBuildEvent.Application.Access;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

internal sealed class OperatorRequestSource : IRequestSource
{
    public string Address => "operator-cli";
}

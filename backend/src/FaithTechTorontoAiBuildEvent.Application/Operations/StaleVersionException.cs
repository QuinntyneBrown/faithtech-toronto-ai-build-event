namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public sealed class StaleVersionException(object current) : Exception
{
    public object Current { get; } = current;
}

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public interface IPrivateConnectionRegistry
{
    void Add(PrivateConnectionRegistration registration);
    bool Remove(string connectionId);
    IReadOnlyCollection<PrivateConnectionRegistration> Snapshot();
}

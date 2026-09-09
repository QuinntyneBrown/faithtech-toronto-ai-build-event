using System.Collections.Concurrent;

namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed class PrivateConnectionRegistry : IPrivateConnectionRegistry
{
    private readonly ConcurrentDictionary<string, PrivateConnectionRegistration> connections = new();

    public void Add(PrivateConnectionRegistration registration) => connections[registration.ConnectionId] = registration;

    public bool Remove(string connectionId) => connections.TryRemove(connectionId, out _);

    public IReadOnlyCollection<PrivateConnectionRegistration> Snapshot() => connections.Values.ToArray();
}

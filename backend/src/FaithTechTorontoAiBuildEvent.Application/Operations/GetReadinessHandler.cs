using MediatR;
using System.Reflection;
namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public sealed class GetReadinessHandler(IReadinessStore store) : IRequestHandler<GetReadinessQuery, ReadinessState>
{
    public async Task<ReadinessState> Handle(GetReadinessQuery request, CancellationToken cancellationToken) =>
        new(await store.IsReady(cancellationToken), typeof(GetReadinessHandler).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);
}

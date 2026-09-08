using System.CommandLine;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class DisableAdministratorCommand : Command
{
    public DisableAdministratorCommand() : base("disable-admin", "Disable an administrator and revoke their sessions.")
    {
        var username = new Argument<string>("username");
        Arguments.Add(username);
        SetAction((result, cancellationToken) => OperatorExecution.Run((services, token) =>
            services.GetRequiredService<ISender>().Send(new Application.Access.DisableAdministratorCommand(
                result.GetRequiredValue(username)), token), cancellationToken));
    }
}

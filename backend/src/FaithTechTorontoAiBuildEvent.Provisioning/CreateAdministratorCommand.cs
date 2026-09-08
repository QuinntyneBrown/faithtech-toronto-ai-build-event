using System.CommandLine;
using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class CreateAdministratorCommand : Command
{
    public CreateAdministratorCommand() : base("create-admin", "Create an administrator; read the password from stdin or a hidden prompt.")
    {
        var username = new Argument<string>("username");
        Arguments.Add(username);
        SetAction((result, cancellationToken) => OperatorExecution.Run((services, token) =>
            services.GetRequiredService<ISender>().Send(new ProvisionAdministratorCommand(
                result.GetRequiredValue(username), SecretInput.ReadPassword()), token), cancellationToken));
    }
}

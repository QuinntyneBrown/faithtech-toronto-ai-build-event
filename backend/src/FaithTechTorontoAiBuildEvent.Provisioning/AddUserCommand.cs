using System.CommandLine;
using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class AddUserCommand : Command
{
    public AddUserCommand() : base("add-user", "Add an administrator; omitted password options select the documented default password.")
    {
        var username = new Argument<string>("username");
        Arguments.Add(username);
        var input = new AdministratorPasswordInput(this, allowDefault: true);
        input.Configure();
        SetAction((result, cancellationToken) => OperatorExecution.Run(async (services, token) =>
        {
            var password = input.Read(result.GetValue(input.Prompt), result.GetValue(input.Stdin));
            var name = result.GetRequiredValue(username);
            await services.GetRequiredService<ISender>().Send(new ProvisionAdministratorCommand(name, password), token);
            OperatorExecution.WriteCommittedResult(() => Console.WriteLine($"Added administrator: {name}"));
        }, cancellationToken, reportTarget: true));
    }
}

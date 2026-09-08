using System.CommandLine;
using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class ResetPasswordCommand : Command
{
    public ResetPasswordCommand() : base("reset-password", "Reset one administrator or --all and revoke their sessions.")
    {
        var username = new Argument<string?>("username") { Arity = ArgumentArity.ZeroOrOne };
        var all = new Option<bool>("--all") { Description = "Reset every administrator, including disabled accounts." };
        Arguments.Add(username);
        Options.Add(all);
        Validators.Add(result =>
        {
            if ((result.GetValue(username) is not null) == result.GetValue(all))
                result.AddError("Supply one username or --all, never both.");
        });
        var input = new AdministratorPasswordInput(this, allowDefault: false);
        input.Configure();
        SetAction((result, cancellationToken) => OperatorExecution.Run(async (services, token) =>
        {
            var password = input.Read(result.GetValue(input.Prompt), result.GetValue(input.Stdin));
            var affected = await services.GetRequiredService<ISender>().Send(
                new ResetAdministratorPasswordsCommand(result.GetValue(username), password), token);
            CommittedResultOutput.Write([$"Reset administrator passwords: {affected.Length}", .. affected]);
        }, cancellationToken, reportTarget: true));
    }
}

using System.CommandLine;
using FaithTechTorontoAiBuildEvent.Application.Operations;
namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class ShowOperationCommand : Command
{
    public ShowOperationCommand() : base("show", "Display a protected preview for an operation on its verified target.")
    {
        var options = new OperatorOptions(); options.AddTo(this); var operation = new Argument<Guid>("operation-id"); Arguments.Add(operation);
        SetAction((parse, token) => OperatorCommandRunner.Run(parse, options, token, (session, _) => {
            var id = parse.GetValue(operation); session.OperationId = id;
            var preview = session.Files.Read<OperatorPreview>($"{id:N}.operation");
            if (preview.Principal.Key != session.Principal.Key) throw new OperationConflictException();
            session.PreviewId = preview.PreviewId; session.Outcome = "previewed";
            return Task.FromResult<object?>(preview);
        }));
    }
}

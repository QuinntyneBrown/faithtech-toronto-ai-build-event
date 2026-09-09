using System.CommandLine;
using FaithTechTorontoAiBuildEvent.Application.Operations;
namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class ReconcileOperationCommand : Command
{
    public ReconcileOperationCommand() : base("reconcile", "Read the durable outcome without repeating a mutation.")
    {
        var options = new OperatorOptions(); options.AddTo(this); var operation = new Argument<Guid>("operation-id"); Arguments.Add(operation);
        SetAction((parse, token) => OperatorCommandRunner.Run(parse, options, token, async (session, cancellation) => {
            var id = parse.GetValue(operation); session.OperationId = id;
            using var operationLock = session.Files.Lock(id);
            if (session.Files.Exists($"{id:N}.operation")) {
                var preview = session.Files.Read<OperatorPreview>($"{id:N}.operation");
                if (preview.Principal.Key != session.Principal.Key) throw new OperationConflictException();
                if (preview.Kind == "migrate") {
                    session.CurrentPreview = preview; session.PreviewId = preview.PreviewId;
                    var outcome = await OperatorRecovery.Resolve(session, 0, false);
                    session.Outcome = outcome?.Outcome ?? "unconfirmed";
                    if (outcome is null) session.ExitCode = 6;
                    return outcome?.Result;
                }
            }
            var result = await session.Sender.Send(new ReconcileEventImportQuery(id), cancellation);
            session.Outcome = result is null ? "unchanged" : "reconciled"; session.ExitCode = result is null ? 1 : 0;
            return result;
        }));
    }
}

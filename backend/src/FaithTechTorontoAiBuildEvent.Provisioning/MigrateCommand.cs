using System.CommandLine;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class MigrateCommand : Command
{
    public MigrateCommand() : base("migrate", "Preview pending migrations on an explicitly selected target.")
    {
        var options = new OperatorOptions(); options.AddTo(this);
        var preview = new Option<bool>("--preview"); var operation = new Option<Guid?>("--operation-id");
        Options.Add(preview); Options.Add(operation);
        Validators.Add(result => { if (!result.GetValue(preview) || result.GetValue(operation) == Guid.Empty) result.AddError("Use --preview and a nonempty operation identity."); });
        SetAction((parse, token) => OperatorCommandRunner.Run(parse, options, token, async (session, cancellation) => {
            var op = parse.GetValue(operation) ?? Guid.NewGuid(); session.OperationId = op;
            using var operationLock = session.Files.Lock(op);
            var result = new OperatorPreview(1, Guid.NewGuid(), op, "migrate", session.Principal, DateTimeOffset.UtcNow,
                OperatorSession.AssemblyHash, null, null, null,
                (await session.Database.Database.GetAppliedMigrationsAsync(cancellation)).ToArray(),
                (await session.Database.Database.GetPendingMigrationsAsync(cancellation)).ToArray());
            session.PreviewId = result.PreviewId; session.Outcome = "previewed";
            session.Files.Write($"{result.PreviewId:N}.preview", result); session.Files.Write($"{op:N}.operation", result);
            return result;
        }));
    }
}

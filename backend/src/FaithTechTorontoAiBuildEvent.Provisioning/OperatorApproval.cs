using FaithTechTorontoAiBuildEvent.Application.Operations;
namespace FaithTechTorontoAiBuildEvent.Provisioning;

public static class OperatorApproval
{
    public static void Verify(OperatorSession session, OperatorPreview saved)
    {
        if (saved.SchemaVersion != 1 || saved.Principal.Key != session.Principal.Key ||
            saved.Principal.Environment != session.Principal.Environment || saved.AssemblyHash != OperatorSession.AssemblyHash)
            throw new OperationConflictException();
    }
}

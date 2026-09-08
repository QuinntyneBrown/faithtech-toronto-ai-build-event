namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public sealed class OperationConflictException() : Exception("An operation identity was reused with different content.");

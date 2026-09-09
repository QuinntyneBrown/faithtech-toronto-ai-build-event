namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record AdministratorAuthenticationResult(bool Authenticated, string? SessionSecret);

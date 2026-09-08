using Microsoft.AspNetCore.Identity;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class AdministratorAccount : IdentityUser<Guid>
{
    public bool Enabled { get; set; } = true;
}

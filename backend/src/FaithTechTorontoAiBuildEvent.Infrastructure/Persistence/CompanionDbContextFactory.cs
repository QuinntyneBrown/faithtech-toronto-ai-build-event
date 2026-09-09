using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

public sealed class CompanionDbContextFactory : IDesignTimeDbContextFactory<CompanionDbContext>
{
    public CompanionDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("FAITHTECH_COMPANION_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=FaithTechTorontoAiBuildEvent;Integrated Security=true;Encrypt=true;TrustServerCertificate=false";
        return new CompanionDbContext(new DbContextOptionsBuilder<CompanionDbContext>().UseSqlServer(connectionString).Options);
    }
}

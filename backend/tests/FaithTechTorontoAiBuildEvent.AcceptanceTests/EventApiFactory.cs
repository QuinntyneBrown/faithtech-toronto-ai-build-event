using FaithTechTorontoAiBuildEvent.Api;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string database = $"FaithTechAcceptance_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FAITHTECH_TEST_SQL") ??
            "Server=.\\SQLEXPRESS;Integrated Security=true;TrustServerCertificate=true");
        connection.InitialCatalog = database;
        builder.UseSetting("ConnectionStrings:EventDatabase", connection.ConnectionString);
        builder.UseSetting("Security:DigestKey", Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EventDbContext>().Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EventDbContext>().Database.EnsureDeletedAsync();
        await DisposeAsync();
    }

    public HttpClient Browser() => CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    public async Task<string> ProvisionAdministrator(string password)
    {
        using var scope = Services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        if (!await roles.RoleExistsAsync("Administrator"))
            Assert.True((await roles.CreateAsync(new IdentityRole<Guid>("Administrator"))).Succeeded);
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AdministratorAccount>>();
        var username = $"operator-{Guid.NewGuid():N}";
        var account = new AdministratorAccount { Id = Guid.NewGuid(), UserName = username };
        Assert.True((await users.CreateAsync(account, password)).Succeeded);
        Assert.True((await users.AddToRoleAsync(account, "Administrator")).Succeeded);
        return username;
    }
}

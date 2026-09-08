using FaithTechTorontoAiBuildEvent.Api.Access;
using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "FaithTech.Antiforgery";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        builder.Services.AddMediatR(options => options.RegisterServicesFromAssemblyContaining<AuthenticateAdministratorCommand>());
        builder.Services.AddDbContext<EventDbContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("EventDatabase") ?? throw new InvalidOperationException("Configure ConnectionStrings:EventDatabase.")));
        builder.Services.AddIdentityCore<AdministratorAccount>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<EventDbContext>();
        builder.Services.AddScoped<IAdministratorStore, SqlAdministratorStore>();
        builder.Services.AddScoped<AdministratorCookieEvents>();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.Cookie.Name = "FaithTech.Admin";
            options.Cookie.Path = "/api/admin";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.SlidingExpiration = false;
            options.EventsType = typeof(AdministratorCookieEvents);
        });
        builder.Services.AddAuthorization();
        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers.XContentTypeOptions = "nosniff";
            await next();
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}

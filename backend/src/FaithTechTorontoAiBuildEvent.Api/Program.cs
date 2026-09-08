using FaithTechTorontoAiBuildEvent.Api.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using FaithTechTorontoAiBuildEvent.Application.Access;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        builder.Services.AddEventInfrastructure(builder.Configuration);
        builder.Services.AddOptions<SecurityOptions>().ValidateOnStart();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IRequestSource, HttpRequestSource>();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
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
        app.UseExceptionHandler();
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

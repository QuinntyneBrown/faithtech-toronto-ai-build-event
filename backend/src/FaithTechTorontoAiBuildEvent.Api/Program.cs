using FaithTechTorontoAiBuildEvent.Api.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure;
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using FaithTechTorontoAiBuildEvent.Application.Access;
using Microsoft.AspNetCore.Authentication.Cookies;
using FaithTechTorontoAiBuildEvent.Api.Hosting;

namespace FaithTechTorontoAiBuildEvent.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args, WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
        });
        builder.Services.AddControllersWithViews();
        builder.Services.AddEventHosting(builder.Configuration);
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
        builder.Services.AddScoped<ParticipantCookieEvents>();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.Cookie.Name = "FaithTech.Admin";
            options.Cookie.Path = "/api/admin";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.SlidingExpiration = false;
            options.EventsType = typeof(AdministratorCookieEvents);
        }).AddCookie("Participant", options =>
        {
            options.Cookie.Name = "FaithTech.Participant";
            options.Cookie.Path = "/api/events";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.SlidingExpiration = false;
            options.EventsType = typeof(ParticipantCookieEvents);
        });
        builder.Services.AddAuthorization();
        var app = builder.Build();
        app.UseForwardedHeaders();
        if (!app.Environment.IsDevelopment()) app.UseHsts();
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers.XContentTypeOptions = "nosniff";
                return Task.CompletedTask;
            });
            if (!context.Request.IsHttps)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { code = "https-required", correlationId = context.TraceIdentifier });
                return;
            }
            await next();
        });
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStaticFiles();
        app.MapControllers();
        app.MapGet("/", () => Results.File(Path.Combine(app.Environment.WebRootPath, "index.html"), "text/html"));
        app.MapFallbackToFile("/events/{*path:nonfile}", "index.html");
        app.MapFallbackToFile("/admin/{*path:nonfile}", "admin/index.html");
        app.Run();
    }
}

using FaithTechTorontoAiBuildEvent.Application.EventState;
using FaithTechTorontoAiBuildEvent.Api.Hubs;
using FaithTechTorontoAiBuildEvent.Api.Security;
using FaithTechTorontoAiBuildEvent.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<GetPublicEventSnapshotQuery>());
builder.Services.AddCompanionInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IEventUpdatePublisher, SignalREventUpdatePublisher>();
builder.Services.AddHostedService<EventChangePublisher>();
builder.Services.AddSingleton<IPrivateConnectionRegistry, PrivateConnectionRegistry>();
builder.Services.AddHostedService<PrivateSessionInvalidationWatcher>();

var application = builder.Build();
application.UseMiddleware<SameOriginMutationMiddleware>();
application.UseDefaultFiles();
application.UseStaticFiles();
application.MapControllers();
application.MapHub<EventUpdatesHub>("/hubs/event-updates");
application.MapHub<AdministratorUpdatesHub>("/api/admin/updates");
application.MapHub<ParticipantUpdatesHub>("/api/participant/updates");
application.Map("/api/{**path}", () => Results.NotFound());
application.Map("/hubs/{**path}", () => Results.NotFound());
application.MapFallbackToFile("index.html");
application.Run();

public partial class Program;

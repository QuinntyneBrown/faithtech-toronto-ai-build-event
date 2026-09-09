using FaithTechTorontoAiBuildEvent.Application.EventState;
using FaithTechTorontoAiBuildEvent.Api.Hubs;
using FaithTechTorontoAiBuildEvent.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IEventUpdatePublisher, SignalREventUpdatePublisher>();
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<GetPublicEventSnapshotQuery>());
builder.Services.AddCompanionInfrastructure(builder.Configuration);

var application = builder.Build();
application.MapControllers();
application.MapHub<EventUpdatesHub>("/hubs/event-updates");
application.Run();

public partial class Program;

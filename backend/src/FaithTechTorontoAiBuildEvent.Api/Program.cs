using FaithTechTorontoAiBuildEvent.Application.EventState;
using FaithTechTorontoAiBuildEvent.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<GetPublicEventSnapshotQuery>());
builder.Services.AddCompanionInfrastructure(builder.Configuration);

var application = builder.Build();
application.MapControllers();
application.Run();

public partial class Program;

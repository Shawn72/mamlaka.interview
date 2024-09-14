using static Mamlaka.API.Configs.Startup;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigureServices(builder.Services, builder.Configuration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
ConfigureApp(app);

app.Run();
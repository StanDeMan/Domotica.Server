using Domotica.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<MainHub>("/current-time");
app.MapHub<Device>("/device");

app.Run();

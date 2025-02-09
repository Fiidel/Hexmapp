using RelayServer.Rooms;
using RelayServer.WebSocketHandler;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<IRoomManager, RoomManager>();
builder.Services.AddTransient<IRoomCodeGenerator, RoomCodeGenerator>();
var app = builder.Build();

app.UseWebSockets();


app.Map("/connect", async (HttpContext context, WebSocketHandler webSocketHandler) =>
{
    await webSocketHandler.HandleConnection(context);
});

app.Run();

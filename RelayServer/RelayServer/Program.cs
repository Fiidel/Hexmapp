using RelayServer.Rooms;
using RelayServer.WebSocketHandler;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddTransient<IRoomCodeGenerator, RoomCodeGenerator>();
var app = builder.Build();

app.UseWebSockets();


app.Map("/connect", async (HttpContext context, WebSocketHandler webSocketHandler) =>
{
    await webSocketHandler.HandleConnection(context);
});

app.Run();

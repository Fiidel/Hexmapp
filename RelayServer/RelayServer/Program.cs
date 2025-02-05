using System.Net.WebSockets;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();


app.Map("/connect", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        while (true)
        {
            var message = $"Datetime: {DateTime.Now}";
            var bytes = Encoding.UTF8.GetBytes(message);
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else if (webSocket.State == WebSocketState.Closed || webSocket.State == WebSocketState.Aborted)
            {
                break;
            }
            Thread.Sleep(1000);
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();

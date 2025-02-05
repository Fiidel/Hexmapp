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
        
        bool joinedRoom = false;
        while (!joinedRoom)
        {
            var buffer = new byte[1024];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                var splitMessage = message.Split(':');
                var msgType = splitMessage[0];

                if (msgType == "CREATE")
                {
                    var roomCode = "XXXXXXXX";
                    joinedRoom = true;
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Room code: {roomCode}")), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (msgType == "JOIN")
                {
                    var msgContent = splitMessage[1];
                    var roomCode = msgContent;
                    Console.WriteLine($"Attempting to join room {roomCode}.");
                    joinedRoom = true;
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Joined room.")), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (msgType == "LEAVE")
                {
                    var msgContent = splitMessage[1];
                    var roomCode = msgContent;
                    Console.WriteLine($"Attempting to leave room {roomCode}.");
                    joinedRoom = true;
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Left room.")), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();

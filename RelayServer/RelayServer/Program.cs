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
                var msgType = message.Split(':')[0];
                var msgContent = message.Split(':')[1];

                if (msgType == "CREATE")
                {
                    var roomCode = GenerateRoomCode();
                    joinedRoom = true;
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Room code: {roomCode}")), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (msgType == "JOIN")
                {
                    var roomCode = msgContent;
                    Console.WriteLine($"Attempting to join room {roomCode}.");
                    joinedRoom = true;
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Joined room.")), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

string GenerateRoomCode()
{
    var symbolArray = "ABCDEFGHIJKLMNOPQRSTUVXYZ0123456789";
    int codeLength = 8;
    char[] roomCode = new char[codeLength];
    Random random = new Random();
    for (int i = 0; i < codeLength; i++)
    {
        roomCode[i] = symbolArray[random.Next(0, symbolArray.Length)];
    }
    return new string(roomCode);
}

app.Run();

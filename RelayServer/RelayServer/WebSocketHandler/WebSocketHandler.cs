using RelayServer.Rooms;
using System.Net.WebSockets;
using System.Text;

namespace RelayServer.WebSocketHandler
{
    public class WebSocketHandler
    {
        private readonly IRoomManager _roomManager;
        
        public WebSocketHandler(IRoomManager roomManager)
        {
            _roomManager = roomManager;
        }


        public async Task HandleConnection(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await ProcessMessages(webSocket);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task ProcessMessages(WebSocket webSocket)
        {
            // ==========================================================
            // TODO: aborted and loss of connectivity connections
            // implement a timer as per https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-9.0#handle-client-disconnects
            // and do clean up in RoomManager (invoke leave room method)
            // ==========================================================


            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024];
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    // ==========================================================
                    // TODO: clean up in RoomManager
                    // ==========================================================

                    await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close message successful, closing", CancellationToken.None);
                    return;
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    var splitMessage = message.Split(':');
                    var msgType = splitMessage[0];

                    try
                    {
                        switch (msgType)
                        {
                            case "CREATE":
                                var roomCode = _roomManager.CreateRoom(webSocket);
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Room code: {roomCode}")), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;

                            case "JOIN":
                                if (splitMessage.Length < 2)
                                {
                                    throw new Exception("Invalid JOIN command");
                                }
                                var joinRoomCode = splitMessage[1];
                                _roomManager.JoinRoom(joinRoomCode, webSocket);
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Joined room.")), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;

                            case "LEAVE":
                                if (splitMessage.Length < 2)
                                {
                                    throw new Exception("Invalid LEAVE command");
                                }
                                var leaveRoomCode = splitMessage[1];
                                await _roomManager.LeaveRoom(leaveRoomCode, webSocket);
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Left room.")), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;

                            case "MESSAGE":
                                if (splitMessage.Length < 3)
                                {
                                    throw new Exception("Invalid MESSAGE command");
                                }
                                var targetRoom = splitMessage[1];
                                var msgContent = splitMessage[2];
                                _roomManager.RelayToRoom(targetRoom, new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgContent)), webSocket);
                                break;

                            default:
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Unknown command.")), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Error: {ex.Message}")), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
    }
}

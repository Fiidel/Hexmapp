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

                using var pulseTimeout = new WebsocketPulseTimeout(webSocket, TimeSpan.FromSeconds(15));
                pulseTimeout.StartTimeout();

                var player = new Player() { Socket = webSocket };
                await ProcessMessages(player, pulseTimeout);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task ProcessMessages(Player player, WebsocketPulseTimeout pulseTimeout)
        {
            try
            {
                while (player.Socket.State == WebSocketState.Open)
                {
                    // ==========================================================
                    // TODO: buffer for longer messages
                    // ==========================================================

                    var buffer = new byte[1024];
                    var receiveResult = await player.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await player.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close message, closing socket", CancellationToken.None);
                        pulseTimeout.StopTimeout();
                        break; // cleanup in the finally block
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
                                    var roomCode = await _roomManager.CreateRoom(player);
                                    await player.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"RC:{roomCode}")), WebSocketMessageType.Text, true, CancellationToken.None);
                                    break;

                                case "JOIN":
                                    if (splitMessage.Length < 2)
                                    {
                                        throw new Exception("Invalid JOIN command");
                                    }
                                    var joinRoomCode = splitMessage[1];
                                    await _roomManager.JoinRoom(joinRoomCode, player);
                                    await player.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Joined room.")), WebSocketMessageType.Text, true, CancellationToken.None);
                                    break;

                                case "LEAVE":
                                    await _roomManager.LeaveCurrentRoom(player);
                                    await player.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Left room.")), WebSocketMessageType.Text, true, CancellationToken.None);
                                    break;

                                case "MSG":
                                    if (splitMessage.Length < 2)
                                    {
                                        throw new Exception("Invalid MSG command");
                                    }
                                    var msgContent = splitMessage[1];
                                    await _roomManager.RelayToRoom(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgContent)), player);
                                    break;

                                case "PING":
                                    pulseTimeout.RefreshTimeout();
                                    break;

                                default:
                                    await player.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Unknown command.")), WebSocketMessageType.Text, true, CancellationToken.None);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            await player.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Error: {ex.Message}")), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await player.Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Error: {ex.Message}")), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            finally
            {
                if (player.Socket.State == WebSocketState.Open || player.Socket.State == WebSocketState.CloseReceived)
                {
                    await player.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing closed socket", CancellationToken.None);
                }

                await _roomManager.LeaveCurrentRoom(player);
            }
        }
    }
}

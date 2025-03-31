using RelayServer.Rooms;
using System.Net.WebSockets;
using System.Numerics;
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
                WebSocket? webSocket = null;
                try
                {
                    webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    using var pulseTimeout = new WebsocketPulseTimeout(webSocket, TimeSpan.FromSeconds(15));
                    pulseTimeout.StartTimeout();

                    var player = new Player() { Socket = webSocket };
                    await ProcessMessages(player, pulseTimeout);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    if (webSocket != null)
                    {
                        CloseWebSocket(webSocket, "Finalizing connection, closing websocket.");
                    }
                }
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
                    WebSocketReceiveResult? receiveResult;

                    try
                    {
                        receiveResult = await player.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    catch (WebSocketException wse)
                    {
                        Console.WriteLine($"WebSocket ReceiveAsync error: {wse.Message}");
                        break; // exit and cleanup in the finally block
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        break;
                    }

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseWebSocket(player.Socket, "Close message received, closing socket.");
                        pulseTimeout.StopTimeout();
                        break; // exit and cleanup in the finally block
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
                                    await SendTextMessageAsync(player.Socket, $"RC:{roomCode}");
                                    player.Nickname = "Game Master";
                                    break;

                                case "JOIN":
                                    if (splitMessage.Length < 3)
                                    {
                                        throw new Exception("Invalid JOIN command");
                                    }
                                    var joinRoomCode = splitMessage[1];
                                    player.Nickname = splitMessage[2];
                                    await _roomManager.JoinRoom(joinRoomCode, player);
                                    await SendTextMessageAsync(player.Socket, "JOIN:ACK");
                                    break;

                                case "LEAVE":
                                    await _roomManager.LeaveCurrentRoom(player);
                                    await SendTextMessageAsync(player.Socket, "LEAVE:ACK");
                                    break;

                                case "CHAT":
                                    if (splitMessage.Length < 2)
                                    {
                                        throw new Exception("Invalid CHAT command");
                                    }
                                    var msgContent = $"CHAT:{player.Nickname}:{string.Join(":", splitMessage.Skip(1))}";
                                    await _roomManager.RelayToRoom(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgContent)), player);
                                    break;

                                case "PING":
                                    pulseTimeout.RefreshTimeout();
                                    break;

                                default:
                                    await SendTextMessageAsync(player.Socket, "ERR:Unknown network command type.");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            await SendTextMessageAsync(player.Socket, $"ERR:{ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SendTextMessageAsync(player.Socket, $"ERR:{ex.Message}");
            }
            finally
            {
                pulseTimeout.StopTimeout();

                try
                {
                    await _roomManager.LeaveCurrentRoom(player);
                    await CloseWebSocket(player.Socket);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private async Task SendTextMessageAsync(WebSocket socket, string message)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async Task CloseWebSocket(WebSocket socket, string message = "Closing websocket connection.")
        {
            try
            {
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, message, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

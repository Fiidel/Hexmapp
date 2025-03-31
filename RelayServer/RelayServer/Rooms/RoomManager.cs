using System.Net.Sockets;
using System.Net.WebSockets;

namespace RelayServer.Rooms
{
    public class RoomManager(IRoomCodeGenerator roomCodeGenerator) : IRoomManager
    {
        private Dictionary<string, Room> _rooms = [];

        public async Task<string> CreateRoom(Player gameMaster)
        {
            if (gameMaster.JoinedRoomCode != null)
            {
                await LeaveCurrentRoom(gameMaster);
            }

            string roomCode = "";
            var unique = false;
            while (!unique)
            {
                roomCode = roomCodeGenerator.GenerateRoomCode();
                unique = !_rooms.ContainsKey(new string(roomCode));
            }
            gameMaster.JoinedRoomCode = roomCode;

            var room = new Room()
            {
                RoomCode = roomCode,
                GameMaster = gameMaster,
                AllPlayers = [gameMaster]
            };
            _rooms.Add(roomCode, room);

            return roomCode;
        }

        public async Task JoinRoom(string roomId, Player player)
        {
            if (player.JoinedRoomCode != null)
            {
                await LeaveCurrentRoom(player);
            }

            if (_rooms.ContainsKey(roomId))
            {
                if (!_rooms[roomId].AllPlayers.Contains(player))
                {
                    player.JoinedRoomCode = roomId;
                    _rooms[roomId].AllPlayers.Add(player);
                }
                else
                {
                    throw new Exception("Player already in room");
                }
            }
            else
            {
                throw new Exception("Room does not exist");
            }
        }

        public async Task LeaveCurrentRoom(Player player)
        {
            if (player.JoinedRoomCode != null)
            {
                if (_rooms.ContainsKey(player.JoinedRoomCode))
                {
                    if (_rooms[player.JoinedRoomCode].AllPlayers.Contains(player))
                    {
                        try
                        {
                            // game master host leaving closes the room
                            if (_rooms[player.JoinedRoomCode].GameMaster == player)
                            {
                                await CloseRoom(player.JoinedRoomCode);
                                return;
                            }
                            else
                            {
                                _rooms[player.JoinedRoomCode].AllPlayers.Remove(player);
                                player.JoinedRoomCode = null;
                                await CloseWebSocketOnLeave(player.Socket);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    else
                    {
                        throw new Exception("Player not in room");
                    }
                }
                else
                {
                    throw new Exception("Room does not exist");
                }
            }
        }

        public async Task RelayToRoom(ArraySegment<byte> message, Player originator, bool excludeOriginator = false)
        {
            if (originator.JoinedRoomCode != null)
            {
                if (_rooms.TryGetValue(originator.JoinedRoomCode, out var room))
                {
                    foreach (var player in room.AllPlayers)
                    {
                        if (excludeOriginator && player == originator)
                        {
                            continue; // skip originator if exclusion is set to true
                        }

                        try
                        {
                            if (player.Socket.State == WebSocketState.Open)
                            {
                                await player.Socket.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }
                else
                {
                    throw new Exception("Room does not exist");
                }
            }
        }

        public async Task CloseRoom(string roomId)
        {
            if (_rooms.ContainsKey(roomId))
            {
                // close any connected sockets
                foreach (var player in _rooms[roomId].AllPlayers)
                {
                    try
                    {
                        player.JoinedRoomCode = null;
                        await CloseWebSocketOnLeave(player.Socket);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }

                _rooms.Remove(roomId);
            }
            else
            {
                throw new Exception("Room does not exist");
            }
        }

        private async Task CloseWebSocketOnLeave(WebSocket socket)
        {
            try
            {
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "LEAVE successful", CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

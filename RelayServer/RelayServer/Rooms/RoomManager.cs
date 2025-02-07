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
                        // game master host leaving closes the room
                        if (_rooms[player.JoinedRoomCode].GameMaster == player)
                        {
                            await CloseRoom(player.JoinedRoomCode);
                            return;
                        }
                        else
                        {
                            await CloseWebSocketOnLeave(player.Socket);
                            _rooms[player.JoinedRoomCode].AllPlayers.Remove(player);
                            player.JoinedRoomCode = null;
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

        public async Task RelayToRoom(ArraySegment<byte> message, Player originator)
        {
            if (originator.JoinedRoomCode != null)
            {
                if (_rooms.ContainsKey(originator.JoinedRoomCode))
                {
                    foreach (var player in _rooms[originator.JoinedRoomCode].AllPlayers)
                    {
                        if (player != originator)
                        {
                            await player.Socket.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
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
                    await CloseWebSocketOnLeave(player.Socket);
                    player.JoinedRoomCode = null;
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
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "LEAVE successful", CancellationToken.None);
            }
        }
    }
}

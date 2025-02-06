using System.Net.Sockets;
using System.Net.WebSockets;

namespace RelayServer.Rooms
{
    public class RoomManager(IRoomCodeGenerator roomCodeGenerator) : IRoomManager
    {
        private Dictionary<string, Room> _rooms = [];

        public string CreateRoom(WebSocket gameMaster)
        {
            string roomCode = "";
            var unique = false;
            while (!unique)
            {
                roomCode = roomCodeGenerator.GenerateRoomCode();
                unique = !_rooms.ContainsKey(new string(roomCode));
            }

            var room = new Room()
            {
                RoomCode = roomCode,
                GameMaster = gameMaster,
                AllPlayers = [gameMaster]
            };
            _rooms.Add(roomCode, room);

            return roomCode;
        }

        public void JoinRoom(string roomId, WebSocket player)
        {
            if (_rooms.ContainsKey(roomId))
            {
                if (!_rooms[roomId].AllPlayers.Contains(player))
                {
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

        public async Task LeaveRoom(string roomId, WebSocket player)
        {
            if (_rooms.ContainsKey(roomId))
            {
                if (_rooms[roomId].AllPlayers.Contains(player))
                {
                    // game master host leaving disconnects all players
                    if (_rooms[roomId].GameMaster == player)
                    {
                        foreach (var p in _rooms[roomId].AllPlayers)
                        {
                            await CloseWebSocketOnLeave(p);
                        }
                        RemoveRoom(roomId);
                        return;
                    }
                    else
                    {
                        await CloseWebSocketOnLeave(player);
                        _rooms[roomId].AllPlayers.Remove(player);
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

        public void RelayToRoom(string roomId, ArraySegment<byte> message, WebSocket originator)
        {
            if (_rooms.ContainsKey(roomId))
            {
                foreach (var player in _rooms[roomId].AllPlayers)
                {
                    if (player != originator)
                    {
                        player.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
            else
            {
                throw new Exception("Room does not exist");
            }
        }

        public void RemoveRoom(string roomId)
        {
            if (_rooms.ContainsKey(roomId))
            {
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

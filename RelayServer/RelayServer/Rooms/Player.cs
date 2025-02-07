using System.Net.WebSockets;

namespace RelayServer.Rooms
{
    public class Player
    {
        public required WebSocket Socket { get; set; }
        public string? JoinedRoomCode { get; set; } = null;
    }
}

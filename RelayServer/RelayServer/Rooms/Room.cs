using System.Net.WebSockets;

namespace RelayServer.Rooms
{
    public class Room
    {
        public string RoomCode { get; set; } = string.Empty;
        public WebSocket? GameMaster { get; set; }
        public List<WebSocket> AllPlayers { get; set; } = new(); /* includes the game master */
    }
}

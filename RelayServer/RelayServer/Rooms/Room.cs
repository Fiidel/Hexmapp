using System.Net.WebSockets;

namespace RelayServer.Rooms
{
    public class Room
    {
        public string RoomCode { get; set; } = string.Empty;
        public required Player GameMaster { get; set; }
        public List<Player> AllPlayers { get; set; } = new(); /* includes the game master */
    }
}

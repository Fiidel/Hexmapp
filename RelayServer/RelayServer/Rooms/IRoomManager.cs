using System.Net.WebSockets;

namespace RelayServer.Rooms
{
    public interface IRoomManager
    {
        Task<string> CreateRoom(Player gameMaster);
        Task JoinRoom(string roomId, Player player);
        Task LeaveCurrentRoom(Player player);
        Task RelayToRoom(ArraySegment<byte> data, Player originator, bool excludeOriginator = false);
        Task CloseRoom(string roomId);
    }
}

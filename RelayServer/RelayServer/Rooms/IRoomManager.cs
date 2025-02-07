using System.Net.WebSockets;

namespace RelayServer.Rooms
{
    public interface IRoomManager
    {
        string CreateRoom(Player gameMaster);
        void JoinRoom(string roomId, Player player);
        Task LeaveCurrentRoom(Player player);
        void RelayToRoom(ArraySegment<byte> data, Player originator);
        Task CloseRoom(string roomId);
    }
}

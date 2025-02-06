using System.Net.WebSockets;

namespace RelayServer.Rooms
{
    public interface IRoomManager
    {
        string CreateRoom(WebSocket gameMaster);
        void JoinRoom(string roomId, WebSocket player);
        Task LeaveRoom(string roomId, WebSocket player);
        void RelayToRoom(string roomId, ArraySegment<byte> data, WebSocket originator);
        void RemoveRoom(string roomId);
    }
}

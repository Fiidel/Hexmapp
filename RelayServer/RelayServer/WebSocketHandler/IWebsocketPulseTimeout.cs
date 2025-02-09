namespace RelayServer.WebSocketHandler
{
    public interface IWebsocketPulseTimeout
    {
        void StartTimeout();
        void RefreshTimeout();
        void StopTimeout();
    }
}

using System.Timers;
using Timer = System.Timers.Timer;

namespace RelayServer.WebSocketHandler
{
    public class WebsocketPulseTimeout : IWebsocketPulseTimeout, IDisposable
    {
        private readonly TimeSpan _pingInterval;
        private Timer _timer;

        public WebsocketPulseTimeout(TimeSpan pingInterval, ElapsedEventHandler eventHandler)
        {
            _pingInterval = pingInterval;
            _timer = new Timer(2 * pingInterval); // wait for 2x the ping interval https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-9.0#handle-client-disconnects
            _timer.Elapsed += eventHandler;
        }

        public void StartTimeout() => _timer.Start();

        public void RefreshTimeout() => _timer.Interval = 2 * _pingInterval.TotalMilliseconds;

        public void StopTimeout() => _timer.Stop();

        public void Dispose() => _timer.Dispose();
    }
}

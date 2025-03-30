// Connection and poll logic taken from Godot docs
// https://docs.godotengine.org/en/stable/classes/class_websocketpeer.html

using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class WsClient : Node
{
    public string RoomCode = "";

    private WebSocketPeer wsPeer = new();
    private string ServerAddress = "ws://localhost:5044/connect";
    public static WsClient Instance { get; private set; }

    // signals for communicating outside this script
    [Signal] public delegate void RoomCodeReceivedEventHandler();
    [Signal] public delegate void ChatMessageReceivedEventHandler(string nickname, string message);


    public override void _Ready()
    {
        Instance = this;

        // disable message polling (initially, the client is not connected)
        SetProcess(false);
    }


    public override void _Process(double delta)
    {
        // =============================================================
        // =============================================================
        // TODO: START PING PULSE KEEPALIVE
        // =============================================================

        wsPeer.Poll();

        if (wsPeer.GetReadyState() == WebSocketPeer.State.Open)
        {
            while (wsPeer.GetAvailablePacketCount() > 0)
            {
                string message = wsPeer.GetPacket().GetStringFromUtf8();
                ProcessMessage(message);
            }
        }
        else if (wsPeer.GetReadyState() == WebSocketPeer.State.Closed)
        {
            SetProcess(false);
        }
    }


    public async void HostGame()
    {
        if (await ConnectToServer())
        {
            RelayMessage("CREATE");
        }
        else
        {
            GD.Print("Failed to connect to server.");
        }
    }


    public async void JoinRoom(string roomCode, string nickname)
    {
        if (await ConnectToServer())
        {
            RelayMessage($"JOIN:{roomCode.ToUpper()}:{nickname}");
        }
        else
        {
            GD.Print("Failed to connect to server.");
        }
    }


    public void LeaveRoom()
    {
        RelayMessage("LEAVE");
        RoomCode = "";
        DisconnectFromServer();
    }


    public void RelayMessage(string message)
    {
        if (wsPeer.GetReadyState() == WebSocketPeer.State.Open)
        {
            wsPeer.SendText(message);
        }
        else
        {
            GD.Print("WebSocket is not in open state.");
        }
    }


    private async Task<bool> ConnectToServer()
    {
        if (wsPeer.GetReadyState() == WebSocketPeer.State.Open)
        {
            GD.Print("Already connected.");
            return true;
        }

        var err = wsPeer.ConnectToUrl(ServerAddress);
        if (err != Error.Ok)
        {
            GD.Print($"Error connecting to server: {err}");
            return false;
        }

        // wait for the websocket to open before proceeding
        for (int i = 0; i < 50; i++) // 5s timeout
        {
            await Task.Delay(100); // delay before checking again
            wsPeer.Poll();
            if (wsPeer.GetReadyState() == WebSocketPeer.State.Open)
            {
                // start polling
                SetProcess(true);
                
                return true;
            }
        }

        GD.Print("WebSocket connection timed out.");
        return false;
    }


    private void DisconnectFromServer()
    {
        wsPeer.Close();
        GD.Print("WebSocket disconnected.");
    }


    private void ProcessMessage(string message)
    {
        GD.Print($"MSG: {message}");

        var messageSplit = message.Split(':');
        switch (messageSplit[0])
        {
            case "RC":
                RoomCode = messageSplit[1];
                EmitSignal(SignalName.RoomCodeReceived);
                break;
            case "CHAT":
                var nickname = messageSplit[1];
                var chatMessage = $"{string.Join(':', messageSplit.Skip(2))}";
                EmitSignal(SignalName.ChatMessageReceived, nickname, chatMessage);
                break;
            default:
                break;
        }
    }
}

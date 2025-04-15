using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    private string currentSceneUid;
    private Node currentScene;

    // persistent, always visible scenes
    private const string leftNavbarSceneUid = "uid://dqahxqpqd1cql";
    private Node leftNavbarInstance;
    private const string chatSceneUid = "uid://dt22gmsms27up";
    private Node chatInstance;

    // persistent module scenes (hex map, player map, etc.)
    private const string mainMenuSceneUid = "uid://5fylr8f87fqr";
    private PackedScene mainMenuScene;
    private const string playerMapSceneUid = "uid://crmhpjvlveoy2";
    private Node playerMapInstance;
    private const string hexMapSceneUid = "uid://cbs7jcluvxucs";
    private Node hexMapInstance;
    
    // other scenes
    private const string roomCodePopupSceneUid = "uid://bfagl48eikacy"; // room code popup scene for hosting game
    private PackedScene roomCodePopupScene;

    public override void _Ready()
    {
        Instance = this;

        // disable auto-quit (properly handle app quit)
        GetTree().AutoAcceptQuit = false;

        // load scenes
        mainMenuScene = GD.Load<PackedScene>(mainMenuSceneUid);
        var mainMenu = mainMenuScene.Instantiate();
        AddChild(mainMenu);
        // load the main menu on startup
        currentScene = mainMenu;

        playerMapInstance = GD.Load<PackedScene>(playerMapSceneUid).Instantiate();
        hexMapInstance = GD.Load<PackedScene>(hexMapSceneUid).Instantiate();

        // listen to scene switch signal
        SignalBus.Instance.SwitchScene += SetCurrentScene;

        // listen to room code received signal
        WsClient.Instance.RoomCodeReceived += DisplayRoomCodePopup;
    }


    public override void _Notification(int notificationCode)
    {
        if (notificationCode == NotificationWMCloseRequest)
        {
            WsClient.Instance.LeaveRoom();
            GD.Print("Quit: Left room.");
            GetTree().Quit();
        }
    }


    public void LoadScenesOnStartup()
    {
        leftNavbarInstance = GD.Load<PackedScene>(leftNavbarSceneUid).Instantiate();
        AddChild(leftNavbarInstance);

        chatInstance = GD.Load<PackedScene>(chatSceneUid).Instantiate();
        AddChild(chatInstance);

        SetCurrentScene(playerMapSceneUid);
    }


    public void SetCurrentScene(string sceneUid)
    {
        if (currentSceneUid == sceneUid)
        {
            return;
        }

        currentSceneUid = sceneUid;

        switch (sceneUid)
        {
            case playerMapSceneUid:
                DeactivateCurrentScene(currentSceneUid);
                AddChild(playerMapInstance);
                currentScene = playerMapInstance;
                break;
            case hexMapSceneUid:
                DeactivateCurrentScene(currentSceneUid);
                AddChild(hexMapInstance);
                currentScene = hexMapInstance;
                break;
            default:
                currentScene.QueueFree();
                var newScene = GD.Load<PackedScene>(sceneUid);
                currentSceneUid = sceneUid;
                currentScene = newScene.Instantiate();
                AddChild(currentScene);
                break;
        }
    }


    private void DeactivateCurrentScene(string sceneUid)
    {
        if (IsAlwaysActiveScene(sceneUid))
        {
            RemoveChild(currentScene);
        }
        else
        {
            currentScene.QueueFree();
        }
    }


    private bool IsAlwaysActiveScene(string sceneUid)
    {
        return sceneUid == playerMapSceneUid || sceneUid == hexMapSceneUid; // add any more always-loaded uids (||)
    }


    // display room code when hosting a game
    private void DisplayRoomCodePopup()
    {
        var roomCodePopup = GD.Load<PackedScene>(roomCodePopupSceneUid).Instantiate();
        AddChild(roomCodePopup);
    }
}

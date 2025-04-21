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

    // other helpful variables
    public Vector2I PlayerMapSize = new Vector2I(1, 1);


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


    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("save"))
        {
            SaveGame();
        }
    }


    public void SaveGame()
    {
        // call all of the save methods of each class that needs to be saved (hex map, player map, ...)
        // which return a Dictionary with all the data
        var hexMapScript = hexMapInstance as HexMap;
        var hexData = hexMapScript.Save();

        var playerMapScript = playerMapInstance as PlayerMap;
        var playerMapData = playerMapScript.Save();

        var saveData = new Godot.Collections.Dictionary<string, Variant>
        {
            {"hexMap", hexData},
            {"playerMap", playerMapData},
            {"timeline", new Godot.Collections.Dictionary<string, Variant>()}
        };

        // save the dictionaries into JSON
        var saveFilePath = ProjectSettings.GlobalizePath($"user://Campaigns/{CampaignManager.Instance.currentCampaignName}/save.json");
        using var saveFile = FileAccess.Open(saveFilePath, FileAccess.ModeFlags.Write);
        
        var jsonString = Json.Stringify(saveData);
        saveFile.StoreLine(jsonString);
    }


    public void LoadScenesOnStartup()
    {
        leftNavbarInstance = GD.Load<PackedScene>(leftNavbarSceneUid).Instantiate();
        AddChild(leftNavbarInstance);

        chatInstance = GD.Load<PackedScene>(chatSceneUid).Instantiate();
        AddChild(chatInstance);

        hexMapInstance = GD.Load<PackedScene>(hexMapSceneUid).Instantiate();
        playerMapInstance = GD.Load<PackedScene>(playerMapSceneUid).Instantiate();
        SetCurrentScene(hexMapSceneUid);
        SetCurrentScene(playerMapSceneUid);
    }


    public void LoadCampaignData(Godot.Collections.Dictionary<string, Variant> data)
    {
        try
        {
            var hexMapData = (Godot.Collections.Dictionary<string, Variant>) data["hexMap"];
            var hexMapScript = hexMapInstance as HexMap;
            hexMapScript.Load(hexMapData);

            var playerMapData = (Godot.Collections.Dictionary<string, Variant>) data["playerMap"];
            var playerMapScript = playerMapInstance as PlayerMap;
            playerMapScript.Load(playerMapData);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading campaign data: {e.Message}");
            return;
        }
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

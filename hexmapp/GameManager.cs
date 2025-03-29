using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    private const string mainMenuSceneUid = "uid://5fylr8f87fqr";
    private const string playerMapSceneUid = "uid://crmhpjvlveoy2";
    private Node playerMapInstance;
    private const string leftNavbarSceneUid = "uid://dqahxqpqd1cql";
    private Node leftNavbarInstance;
    private const string chatSceneUid = "uid://dt22gmsms27up";
    private Node chatInstance;
    private Node currentScene;
    private string currentSceneUid;
    private PackedScene mainMenuScene;
    private PackedScene playerMapScene;
    // room code popup scene for hosting game
    private const string roomCodePopupSceneUid = "uid://bfagl48eikacy";
    private PackedScene roomCodePopupScene;


    public override void _Ready()
    {
        Instance = this;
        
        mainMenuScene = GD.Load<PackedScene>(mainMenuSceneUid);
        var mainMenu = mainMenuScene.Instantiate();
        AddChild(mainMenu);
        // load the main menu on startup
        currentScene = mainMenu;

        playerMapScene = GD.Load<PackedScene>(playerMapSceneUid);
        playerMapInstance = playerMapScene.Instantiate();

        // listen to room code received signal
        WsClient.Instance.RoomCodeReceived += DisplayRoomCodePopup;
    }


    public void LoadCampaign()
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
        return sceneUid == playerMapSceneUid; // add any more always-loaded uids (||)
    }


    // display room code when hosting a game
    private void DisplayRoomCodePopup()
    {
        var roomCodePopup = GD.Load<PackedScene>(roomCodePopupSceneUid).Instantiate();
        AddChild(roomCodePopup);
    }
}

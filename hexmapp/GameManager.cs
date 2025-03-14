using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    private const string mainMenuSceneUid = "uid://5fylr8f87fqr";
    private const string playerMapSceneUid = "uid://crmhpjvlveoy2";
    private Node playerMapInstance;
    private Node currentScene;
    private string currentSceneUid;
    private PackedScene mainMenuScene;
    private PackedScene playerMapScene;


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
}

using Godot;
using System;

public partial class SignalBus : Node
{
    public static SignalBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    // MainMenu
    [Signal] public delegate void MainMenu_CloseJoinCampaignPopupEventHandler();

    // PlayerMap
    [Signal] public delegate void ClickedPlayerMapObjectEventHandler(Area2D playerMapObject);
    [Signal] public delegate void ClickedPlayerMapBackgroundEventHandler();
}

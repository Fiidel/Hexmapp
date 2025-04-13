using Godot;
using System;

public partial class MainMenu : Control
{
    private Panel createCampaignPopup;
    private Panel loadCampaignPopup;
    private Panel joinRoomPopup;

    public override void _Ready()
    {
        createCampaignPopup = GetNode<Panel>("%CreateCampaignPopup");
        loadCampaignPopup = GetNode<Panel>("%LoadCampaignPopup");
        joinRoomPopup = GetNode<Panel>("%JoinCampaignPopup");
        

        if (joinRoomPopup == null)
        {
            GD.Print("joinRoomPopup failed to load.");
        }
    }

    private void OnCreateCampaignButtonPressed()
    {
        createCampaignPopup.Visible = true;
    }

    private void OnLoadCampaignButtonPressed()
    {
        loadCampaignPopup.Visible = true;
    }

    private void OnJoinCampaignButtonPressed()
    {
        joinRoomPopup.Visible = true;
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
        GetTree().Quit();
    }
}

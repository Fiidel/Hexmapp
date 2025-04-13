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

        // signals
        SignalBus.Instance.MainMenu_CloseJoinCampaignPopup += () => HidePopup(joinRoomPopup);
    }

    private void OnCreateCampaignButtonPressed()
    {
        GameManager.Instance.LoadCampaign();
    }

    private void OnJoinCampaignButtonPressed()
    {
        joinRoomPopup.Visible = true;
    }

    private void HidePopup(Panel popup)
    {
        popup.Visible = false;
    }
}

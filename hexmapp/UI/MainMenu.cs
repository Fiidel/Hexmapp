using Godot;
using System;

public partial class MainMenu : Control
{
    private Panel roomCodePopup;
    private LineEdit roomCodeInput;

    public override void _Ready()
    {
        roomCodePopup = GetNode<Panel>("%RoomCodePopup");
        roomCodeInput = roomCodePopup.GetNode<LineEdit>("%RoomCodeInput");

        if (roomCodePopup == null)
        {
            GD.Print("roomCodePopup failed to load.");
        }
        if (roomCodeInput == null)
        {
            GD.Print("roomCodeInput failed to load.");
        }
    }

    private void OnCreateCampaignButtonPressed()
    {
        GameManager.Instance.LoadCampaign();
    }

    private void OnJoinCampaignButtonPressed()
    {
        roomCodePopup.Visible = true;
    }

    private void OnRoomCodePopupOk()
    {
        WsClient.Instance.JoinRoom(roomCodeInput.Text);
        
        // TODO: load actual campaign data instead
        GameManager.Instance.LoadCampaign();
    }

    private void OnRoomCodePopupCancel()
    {
        roomCodeInput.Text = "";
        roomCodePopup.Visible = false;
    }
}

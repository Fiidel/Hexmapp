using Godot;
using System;

public partial class MainMenu : Control
{
    private Panel roomCodePopup;
    private LineEdit nicknameInput;
    private LineEdit roomCodeInput;

    public override void _Ready()
    {
        roomCodePopup = GetNode<Panel>("%RoomCodePopup");
        roomCodeInput = roomCodePopup.GetNode<LineEdit>("%RoomCodeInput");
        nicknameInput = GetNode<LineEdit>("%NicknameInput");

        if (roomCodePopup == null)
        {
            GD.Print("roomCodePopup failed to load.");
        }
        if (roomCodeInput == null)
        {
            GD.Print("roomCodeInput failed to load.");
        }
        if (nicknameInput == null)
        {
            GD.Print("nicknameInput failed to load.");
        }

        roomCodeInput.TextChanged += (_) => ResetInputColorToDefault(roomCodeInput);
        nicknameInput.TextChanged += (_) => ResetInputColorToDefault(nicknameInput);
    }

    private void OnCreateCampaignButtonPressed()
    {
        GameManager.Instance.LoadCampaign();
    }

    private void OnJoinCampaignButtonPressed()
    {
        roomCodePopup.Visible = true;
    }

    private void ResetInputColorToDefault(LineEdit lineEdit)
    {
        lineEdit.Modulate = new Color(1, 1, 1, 1);
    }

    private void OnRoomCodePopupOk()
    {
        // validate input
        bool invalid = false;
        if (roomCodeInput.Text == "" || roomCodeInput.Text.Length != 8)
        {
            invalid = true;
            roomCodeInput.Modulate = new Color(1, 0, 0, 1);
        }
        if (nicknameInput.Text == "" || nicknameInput.Text.Contains(':'))
        {
            invalid = true;
            nicknameInput.Modulate = new Color(1, 0, 0, 1);
        }
        if (invalid)
        {
            return;
        }

        // join room
        WsClient.Instance.JoinRoom(roomCodeInput.Text, nicknameInput.Text);
        
        // TODO: load actual campaign data instead
        GameManager.Instance.LoadCampaign();
    }

    private void OnRoomCodePopupCancel()
    {
        roomCodeInput.Text = "";
        roomCodePopup.Visible = false;
    }
}

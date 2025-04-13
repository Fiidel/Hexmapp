using Godot;
using System;

public partial class JoinCampaignPopup : Panel
{
    private LineEdit nicknameInput;
    private LineEdit roomCodeInput;

    public override void _Ready()
    {
        roomCodeInput = GetNode<LineEdit>("%RoomCodeInput");
        nicknameInput = GetNode<LineEdit>("%NicknameInput");

        roomCodeInput.TextChanged += (_) => ResetInputColorToDefault(roomCodeInput);
        nicknameInput.TextChanged += (_) => ResetInputColorToDefault(nicknameInput);
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
        nicknameInput.Text = "";
        ResetInputColorToDefault(nicknameInput);
        roomCodeInput.Text = "";
        ResetInputColorToDefault(roomCodeInput);
        SignalBus.Instance.EmitSignal(SignalBus.SignalName.MainMenu_CloseJoinCampaignPopup);
    }
}

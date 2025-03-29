using Godot;
using System;

public partial class RoomCodePopup : Panel
{
    public override void _Ready()
    {
        GetNode<RichTextLabel>("%RoomCodeDisplay").Text = WsClient.Instance.RoomCode;
    }

    private void OnRoomCodePopupOk()
    {
        QueueFree();
    }
}

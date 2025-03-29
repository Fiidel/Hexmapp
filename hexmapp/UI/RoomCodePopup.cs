using Godot;
using System;

public partial class RoomCodePopup : CanvasLayer
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

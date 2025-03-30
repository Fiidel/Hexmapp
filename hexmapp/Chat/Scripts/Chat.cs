using Godot;
using System;

public partial class Chat : CanvasLayer
{
    private LineEdit chatInput;
    private RichTextLabel chatLog;

    public override void _Ready()
    {
        chatInput = GetNode<LineEdit>("%ChatInput");
        chatLog = GetNode<RichTextLabel>("%ChatLog");

        WsClient.Instance.ChatMessageReceived += ReceiveMessage;
    }

    private void SendMessage()
    {
        string message = chatInput.Text;
        if (message != "")
        {
            WsClient.Instance.RelayMessage($"CHAT:{message}");
            chatInput.Text = "";
        }
    }

    private void ReceiveMessage(string nickname, string message)
    {
        chatLog.AddText($"{nickname}: ");        
        chatLog.AddText($"{message}\n");
    }
}

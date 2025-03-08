using Godot;
using System;

public partial class MapObject : Area2D
{
    public override void _Ready()
    {
        InputEvent += OnInputEvent;
    }


    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsActionPressed("left_click"))
        {
            GetViewport().SetInputAsHandled();
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.ClickedPlayerMapObject, this);
        }
    }
}

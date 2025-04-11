using Godot;
using System;

public partial class NoteEntry : Control
{
    private bool isWindowDraggable = false;
    private bool dragging = false;

    private void CloseNote()
    {
        QueueFree();
    }

    // connected with bool parameter via editor
    private void IsDraggable(bool draggable)
    {
        isWindowDraggable = draggable;
    }

    public override void _Input(InputEvent @event)
    {
        if (isWindowDraggable && @event.IsActionPressed("left_click"))
        {
            dragging = true;
        }
        else if (dragging && @event is InputEventMouseMotion mouseMotion)
        {
            Position += mouseMotion.Relative;
        }
        else if (dragging && @event.IsActionReleased("left_click"))
        {
            dragging = false;
        }
    }
}

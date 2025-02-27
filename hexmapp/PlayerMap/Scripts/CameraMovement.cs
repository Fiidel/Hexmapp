using Godot;
using System;

public partial class CameraMovement : Camera2D
{
	[Export]
	private float currentZoom = 0.5f;
	[Export]
	private float minZoom = 0.125f;
	[Export]
	private float maxZoom = 1f;
	[Export]
	private float zoomStep = 0.125f;
	private bool isPanning;


    public override void _UnhandledInput(InputEvent @event)
    {
		// panning button press
		if (@event.IsActionPressed("middle_mouse_click"))
		{
			isPanning = true;
		}
		if (@event.IsActionReleased("middle_mouse_click"))
		{
			isPanning = false;
		}

		// zoom input handling
        if (@event is InputEventMouseButton mouse)
		{
			if (mouse.ButtonIndex == MouseButton.WheelUp)
			{
				SetZoom(currentZoom + zoomStep);
			}
			else if (mouse.ButtonIndex == MouseButton.WheelDown)
			{
				SetZoom(currentZoom - zoomStep);
			}
		}

		// panning
		if (isPanning && @event is InputEventMouseMotion mouseMotion)
		{
			Position -= mouseMotion.Relative / Zoom;
		}
    }

	private void SetZoom(float zoom)
	{
		currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
		Zoom = new Vector2(currentZoom, currentZoom);
	}
}

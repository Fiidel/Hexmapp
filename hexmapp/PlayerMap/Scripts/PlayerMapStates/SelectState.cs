using Godot;

public class SelectState : IPlayerMapState
{
    private PlayerMap context;
    private Area2D selectedObject;
    private bool isDragging;
    private RectangleOutline currentSelectionOutline;

    public SelectState(PlayerMap playerMap)
    {
        context = playerMap;
    }


    public void EnterState()
    {
        SignalBus.Instance.ClickedPlayerMapObject += OnMapObjectClicked;
    }


    public void ExitState()
    {
        SignalBus.Instance.ClickedPlayerMapObject -= OnMapObjectClicked;
        selectedObject = null;
        RemoveRectOutline();
    }


    public void ProcessInput(InputEvent @event)
    {
        if (selectedObject != null)
        {
            if (Input.IsActionPressed("left_click") && @event is InputEventMouseMotion)
            {
                isDragging = true;
                selectedObject.GlobalPosition = context.GetGlobalMousePosition();
            }
            else if (isDragging && @event.IsActionReleased("left_click"))
            {
                isDragging = false;
                var moveMapObjectCommand = new MoveMapObjectCommand(selectedObject, context.GetGlobalMousePosition());
		        moveMapObjectCommand.Execute();
            }
        }
    }


    private void OnMapObjectClicked(Area2D mapObject)
    {
        selectedObject = mapObject;

        // selection rectangle
        var objectTexture = mapObject.GetNode<Sprite2D>("Texture").Texture;
        RemoveRectOutline();
        AddRectOutline(mapObject, objectTexture);
    }


    private void AddRectOutline(Area2D objectToOutline, Texture2D objectTexture)
	{
		currentSelectionOutline = new RectangleOutline(objectTexture);
		objectToOutline.AddChild(currentSelectionOutline);
	}


	private void RemoveRectOutline()
	{
		if (currentSelectionOutline != null)
		{
			currentSelectionOutline.QueueFree();
			currentSelectionOutline = null;
		}
	}
}
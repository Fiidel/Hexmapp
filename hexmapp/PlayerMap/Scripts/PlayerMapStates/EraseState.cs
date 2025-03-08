using Godot;

public class EraseState : IPlayerMapState
{
    private PlayerMap context;
    private EraseTilesCommand currentEraseTilesCommand = null;
    private bool isTileErasing = false;

    public EraseState(PlayerMap playerMap)
    {
        context = playerMap;
    }


	public void EnterState() { }


	public void ExitState() { }


    public void ProcessInput(InputEvent @event)
    {
        if (@event.IsActionPressed("left_click"))
		{
            StartErasingTiles();
        }
        else if (isTileErasing && @event is InputEventMouseMotion)
		{
			ContinueErasingTiles();
		}
        else if (isTileErasing && @event.IsActionReleased("left_click"))
		{
			StopErasingTiles();
		}
    }


    private void StartErasingTiles()
	{
		var clickedTile = context.GetAndLogTileIndex();
		// for continous erasing
		isTileErasing = true;

		// create a new command that accumulates erased tiles
		currentEraseTilesCommand = context.CreateEraseTilesCommand();
		// draw the first tile
		AddTileToCommandAndErase(clickedTile);
	}


	private void ContinueErasingTiles()
	{
		var currentTile = context.GetTileIndex();
		if (context.lastTileIndex != currentTile)
		{
			context.lastTileIndex = currentTile;
			AddTileToCommandAndErase(currentTile);
		}
	}


	private void StopErasingTiles()
	{
		isTileErasing = false;
		context.lastTileIndex = new Vector2I(-1, -1);

		// execute the whole erase command (in case of being overwritten by another player's command in the meantime of being drawn)
		currentEraseTilesCommand.Execute();
		currentEraseTilesCommand = null;
	}


	private void AddTileToCommandAndErase(Vector2I tileIndex)
	{
		currentEraseTilesCommand.tileIndices.Add(tileIndex);
		context.tileMapDrawTool.BrushEraseTiles(tileIndex, context.GetBrushSize());
	}
}
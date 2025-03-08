using Godot;

public class DrawState : IPlayerMapState
{
    private PlayerMap context;
    private DrawTilesCommand currentDrawTilesCommand = null;
    private bool isTileDrawing = false;

    public DrawState(PlayerMap playerMap)
    {
        context = playerMap;
    }


	public void EnterState() { }


	public void ExitState() { }


    public void ProcessInput(InputEvent @event)
    {
        // start drawing tiles with the selected tile brush
		if (@event.IsActionPressed("left_click"))
		{
			if (context.GetSelectedTool() is Tile tile)
			{
				StartDrawingTiles(tile);
			}
			else if (context.GetSelectedTool() is MapAsset mapAsset)
			{
				context.PlaceMapAsset(mapAsset);
			}
			else if (context.GetSelectedTool() is MapPin mapPin)
			{
				context.PlaceMapPin(mapPin);
			}
		}

        // continous drawing while the left mouse button is down and moving
        else if (isTileDrawing && @event is InputEventMouseMotion)
		{
			if (context.GetSelectedTool() is Tile tile)
			{
				ContinueDrawingTiles(tile);
			}
        }

        // stop drawing when the left mouse button is released
        else if (isTileDrawing && @event.IsActionReleased("left_click"))
		{
			StopDrawingTiles();
		}
    }


    private void StartDrawingTiles(Tile tile)
    {
		var clickedTile = context.GetAndLogTileIndex();
		// for continous drawing
		isTileDrawing = true;

        // create a new command that accumulates drawn tiles
        currentDrawTilesCommand = context.CreateDrawTilesCommand(tile);
        // add the first tile into the command and draw
		AddTileToCommandAndDraw(clickedTile, tile);
    }


	private void ContinueDrawingTiles(Tile tile)
    {
        var currentTile = context.GetTileIndex();
		if (context.lastTileIndex != currentTile)
		{
			context.lastTileIndex = currentTile;
			AddTileToCommandAndDraw(currentTile, tile);
		}
    }


    private void StopDrawingTiles()
    {
        isTileDrawing = false;
		context.lastTileIndex = new Vector2I(-1, -1);

		// execute the whole tile draw command (in case of being overwritten by another player's command in the meantime of being drawn)
		currentDrawTilesCommand.Execute();
		currentDrawTilesCommand = null;
    }


    private void AddTileToCommandAndDraw(Vector2I tileIndex, Tile tile)
	{
		currentDrawTilesCommand.tileIndices.Add(tileIndex);
		context.tileMapDrawTool.BrushDrawTiles(tileIndex, tile, context.GetBrushSize());
	}
}
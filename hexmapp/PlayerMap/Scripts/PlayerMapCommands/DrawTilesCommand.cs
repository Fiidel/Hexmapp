using Godot;
using System.Collections.Generic;

public class DrawTilesCommand : Command
{
    public List<Vector2I> tileIndices;
    private int brushSize;
    private Tile tileType;
	private TileMapDrawTool drawTool;

    public DrawTilesCommand(BaseMapData baseMapData, Dictionary<string, TileMapLayer> displayLayersDict, 
        List<Vector2I> tileIndices, int brushSize, Tile tileType)
    {
        this.tileIndices = tileIndices;
        this.brushSize = brushSize;
        this.tileType = tileType;
		drawTool = new TileMapDrawTool(baseMapData, displayLayersDict);
    }

    public override void Execute()
    {
        for (int i = 0; i < tileIndices.Count; i++)
        {
            drawTool.BrushDrawTiles(tileIndices[i], tileType, brushSize);
        }
    }
}
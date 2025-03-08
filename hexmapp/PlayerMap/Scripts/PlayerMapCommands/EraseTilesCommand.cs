using Godot;
using System.Collections.Generic;

public class EraseTilesCommand : Command
{
    public List<Vector2I> tileIndices;
    private int brushSize;
	private TileMapDrawTool drawTool;

    public EraseTilesCommand(BaseMapData baseMapData, Dictionary<string, TileMapLayer> displayLayersDict, 
        List<Vector2I> tileIndices, int brushSize)
    {
        this.tileIndices = tileIndices;
        this.brushSize = brushSize;
		drawTool = new TileMapDrawTool(baseMapData, displayLayersDict);
    }

    public override void Execute()
    {
        for (int i = 0; i < tileIndices.Count; i++)
        {
            drawTool.BrushEraseTiles(tileIndices[i], brushSize);
        }
    }
}
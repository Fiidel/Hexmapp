using Godot;
using System.Collections.Generic;

public class DrawTilesCommand : Command
{
    private BaseMapData baseMapData;
    private Dictionary<string, TileMapLayer> displayLayersDict;
    public List<Vector2I> tileIndices;
    private int brushSize;
    private Tile tileType;

    public DrawTilesCommand(BaseMapData baseMapData, Dictionary<string, TileMapLayer> displayLayersDict, 
        List<Vector2I> tileIndices, int brushSize, Tile tileType)
    {
        this.baseMapData = baseMapData;
        this.displayLayersDict = displayLayersDict;
        this.tileIndices = tileIndices;
        this.brushSize = brushSize;
        this.tileType = tileType;
    }

    public override void Execute()
    {
        for (int i = 0; i < tileIndices.Count; i++)
        {
            BrushDrawTiles(tileIndices[i], tileType);
        }
    }

    // Draw using the brush size
	public void BrushDrawTiles(Vector2I tileIndex, Tile tileType)
	{
		int brushRadius = brushSize/2;
		int roundnessFactor = brushSize/10 + 1;

		for (int x = -brushRadius; x <= brushRadius; x++)
		{
			for (int y = -brushRadius; y <= brushRadius; y++)
			{
				// skip tiles outside of a circular radius
				if (x*x + y*y > brushRadius*brushRadius + roundnessFactor)
				{
					continue;
				}
				AddTile(new Vector2I(x + tileIndex.X, y + tileIndex.Y), tileType);
			}
		}
	}

    // Assign the selected tile texture to the clicked tile on the base grid
	private void AddTile(Vector2I tileIndex, Tile tileType)
	{
		if (tileIndex.X < 0 || tileIndex.X >= baseMapData.mapWidth || tileIndex.Y < 0 || tileIndex.Y >= baseMapData.mapHeight)
		{
			return;
		}
		baseMapData.baseTiles[tileIndex.X, tileIndex.Y] = tileType;
		UpdateDisplayLayers(tileIndex);
	}

	// Choose the alpha tiles for the 4 surrounding tiles on all the display layers
	private void UpdateDisplayLayers(Vector2I tileIndex)
	{
		foreach (TileMapLayer layer in displayLayersDict.Values)
		{
			Tile tileType = (Tile)layer.GetMeta("Tile");
			int atlasIndex;
			
			// for alpha cell [0,0]
			atlasIndex = CalculateAtlasIndex(tileType, tileIndex, -1, -1);
			layer.SetCell(tileIndex, atlasIndex, Vector2I.Zero);

			// for alpha cell [1,0]
			atlasIndex = CalculateAtlasIndex(tileType, tileIndex, 0, -1);
			layer.SetCell(tileIndex + Vector2I.Right, atlasIndex, Vector2I.Zero);

			// for alpha cell [0,1]
			atlasIndex = CalculateAtlasIndex(tileType, tileIndex, -1, 0);
			layer.SetCell(tileIndex + Vector2I.Down, atlasIndex, Vector2I.Zero);

			// for alpha cell [1,1]
			atlasIndex = CalculateAtlasIndex(tileType, tileIndex, 0, 0);
			layer.SetCell(tileIndex + Vector2I.One, atlasIndex, Vector2I.Zero);
		}
	}

    private bool IsTileTypeSame(Tile tileType, int x, int y)
	{
		// out of bounds
		if (x < 0 || x >= baseMapData.mapWidth || y < 0 || y >= baseMapData.mapHeight)
		{
			return false;
		}
		return baseMapData.baseTiles[x, y] == tileType;
	}

	// xOffset and yOffset signify where the 1st checked base tile starts as compared
	// to the center base tile that has been drawn (always the top left one)
	private int CalculateAtlasIndex(Tile tileType, Vector2I tileIndex, int xOffset, int yOffset)
	{
		int atlasIndex = 0;

		if (IsTileTypeSame(tileType, tileIndex.X + xOffset, tileIndex.Y + yOffset))
		{
			atlasIndex += 8;
		}
		if (IsTileTypeSame(tileType, tileIndex.X + xOffset + 1, tileIndex.Y + yOffset))
		{
			atlasIndex += 4;
		}
		if (IsTileTypeSame(tileType, tileIndex.X + xOffset, tileIndex.Y + yOffset + 1))
		{
			atlasIndex += 2;
		}
		if (IsTileTypeSame(tileType, tileIndex.X + xOffset + 1, tileIndex.Y + yOffset + 1))
		{
			atlasIndex += 1;
		}

		return atlasIndex;
	}
}
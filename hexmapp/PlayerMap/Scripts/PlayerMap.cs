using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerMap : Node2D
{
	private TerrainToolsUi terrainToolsUi;
	private Node terrainGrids;
	private TileMapLayer baseLayer;
	private Tile[,] baseTiles;
	private int mapWidth = 20;
	private int mapHeight = 15;
	private TileMapLayer displayLayer;
	private Dictionary<string, TileMapLayer> displayLayersDict = new();

	public override void _Ready()
	{
		// get nodes and resources
		terrainToolsUi = GetNode<TerrainToolsUi>("TerrainToolsUI");
		terrainGrids = GetNode<Node>("TerrainGrids");
		baseLayer = GetNode<TileMapLayer>("TerrainGrids/BaseTerrainGrid");
		baseTiles = new Tile[mapWidth,mapHeight];
		displayLayer = GetNode<TileMapLayer>("TerrainGrids/DisplayTerrainOffsetGrid");

		// check for missing data and errors
		if (terrainToolsUi == null)
		{
			GD.Print("Terrain tools UI not found.");
			return;
		}
		if (terrainGrids == null)
		{
			GD.Print("Terrain grids not found.");
			return;
		}
		if (baseLayer == null)
		{
			GD.Print("Base tile map layer not found.");
			return;
		}
		if (displayLayer == null)
		{
			GD.Print("Display tile map layer not found.");
			return;
		}
	}

	// TODO: draw 1 tile with the selected tile brush
	
	// Detect base tile indices on click
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("left_click"))
		{
			var clickedTile = baseLayer.LocalToMap(GetGlobalMousePosition());
			var tileLayer = GetOrCreateTileMapLayer(terrainToolsUi.SelectedTile.IdName);
			AddTile(clickedTile, terrainToolsUi.SelectedTile);
			
			GD.Print($"Tile: {clickedTile}. Terrain: {terrainToolsUi.SelectedTile.IdName}. Layer count: {displayLayersDict.Count}.");
		}
	}

	// Get or create a tile map layer
	private TileMapLayer GetOrCreateTileMapLayer(string tileName)
	{
		// if the tile hasn't been used yet, create a new layer
		if (!displayLayersDict.ContainsKey(tileName))
		{
			TileMapLayer newLayer = displayLayer.Duplicate() as TileMapLayer;
			newLayer.Name = $"{tileName}Layer";
			newLayer.SetMeta("Tile", terrainToolsUi.SelectedTile);
			for (int x = 0; x <= mapWidth; x++) // <= ... to account for the offset to top left by half a tile
			{
				for (int y = 0; y <= mapHeight; y++)
				{
					newLayer.SetCell(new Vector2I(x, y), 0, new Vector2I(0,0));
				}
			}

			// add to scene and dictionary
			terrainGrids.AddChild(newLayer);
			displayLayersDict.Add(tileName, newLayer);
			return newLayer;
		}
		// otherwise get the existing layer for that tile
		return displayLayersDict[tileName];
	}

	// Assign the selected tile texture to the clicked tile on the base grid
	private void AddTile(Vector2I tileIndex, Tile tileType)
	{
		if (tileIndex.X < 0 || tileIndex.X >= mapWidth || tileIndex.Y < 0 || tileIndex.Y >= mapHeight)
		{
			return;
		}
		baseTiles[tileIndex.X, tileIndex.Y] = tileType;
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
			layer.SetCell(tileIndex, 0, new Vector2I(atlasIndex, 0));

			// for alpha cell [1,0]
			atlasIndex = CalculateAtlasIndex(tileType, tileIndex, 0, -1);
			layer.SetCell(tileIndex + Vector2I.Right, 0, new Vector2I(atlasIndex, 0));

			// for alpha cell [0,1]
			atlasIndex = CalculateAtlasIndex(tileType, tileIndex, -1, 0);
			layer.SetCell(tileIndex + Vector2I.Down, 0, new Vector2I(atlasIndex, 0));

			// for alpha cell [1,1]
			atlasIndex = CalculateAtlasIndex(tileType, tileIndex, 0, 0);
			layer.SetCell(tileIndex + Vector2I.One, 0, new Vector2I(atlasIndex, 0));
		}
	}

	private bool IsTileTypeSame(Tile tileType, int x, int y)
	{
		// TODO: need to account for checking non-existent base tiles outside of the grid size!!!!!!!!!!!!!!!!!!!!!!!
		if (baseTiles[x, y] == tileType)
		{
			return true;
		}
		return false;
	}

	// xOffset and yOffset signify where the 1st checked base tile starts as compared
	// to the center base tile that has been drawn (always the top left one)
	int CalculateAtlasIndex(Tile tileType, Vector2I tileIndex, int xOffset, int yOffset)
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





	// B. implement strokes (rather than just setting 1 tile)
	// C. implement erasing tiles
	// D. implement it as a command that gathers all drawn tiles
	// E. implement brush sizes (which will need to account for setting nonexistent tiles at the edge of the base terrain index)
}

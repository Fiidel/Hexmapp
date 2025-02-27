using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerMap : Node2D
{
	private TerrainToolsUi terrainToolsUi;
	private Node terrainGrids;
	private TileMapLayer baseLayer;
	private Tile[,] baseTiles;
	private int mapWidth = 200;
	private int mapHeight = 150;
	private TileMapLayer displayLayer;
	private Dictionary<string, TileMapLayer> displayLayersDict = new();
	private bool isDrawing;
	private Vector2I lastTileIndex = new Vector2I(-1, -1);
	private HSlider brushSlider;
	private int brushSize = 1;
	private Shader alphaShader = GD.Load<Shader>("res://PlayerMap/alpha_mask.gdshader");

	public override void _Ready()
	{
		// get nodes and resources
		baseLayer = GetNode<TileMapLayer>("TerrainGrids/BaseTerrainGrid");
		baseTiles = new Tile[mapWidth,mapHeight];
		displayLayer = GetNode<TileMapLayer>("TerrainGrids/DisplayTerrainOffsetGrid");
		terrainToolsUi = GetNode<TerrainToolsUi>("TerrainToolsUI");
		terrainGrids = GetNode<Node>("TerrainGrids");
		brushSlider = GetNode<HSlider>("%TerrainBrushSizeSlider");

		// check for missing data and errors
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
		if (brushSlider == null)
		{
			GD.Print("Terrain brush slider not found.");
			return;
		}

		// connect signals
		brushSlider.DragEnded += OnBrushSizeChanged;
	}

	public override void _ExitTree()
    {
        base._ExitTree();
		brushSlider.DragEnded -= OnBrushSizeChanged;
    }

    // Detect base tile indices on click
    public override void _UnhandledInput(InputEvent @event)
	{
		// start drawing tiles with the selected tile brush
		if (@event.IsActionPressed("left_click"))
		{
			var clickedTile = baseLayer.LocalToMap(GetGlobalMousePosition());
			GetOrCreateTileMapLayer(terrainToolsUi.SelectedTile.IdName);
			BrushDrawTiles(clickedTile, terrainToolsUi.SelectedTile);

			// for continous drawing
			isDrawing = true;
			lastTileIndex = clickedTile;
			
			GD.Print($"Tile: {clickedTile}. Terrain: {terrainToolsUi.SelectedTile.IdName}. Layer count: {displayLayersDict.Count}.");
		}
		// continous drawing while the left mouse button is down and moving
		else if (isDrawing && @event is InputEventMouseMotion)
		{
			var currentTile = baseLayer.LocalToMap(GetGlobalMousePosition());
			if (lastTileIndex != currentTile)
			{
				lastTileIndex = baseLayer.LocalToMap(GetGlobalMousePosition());
				BrushDrawTiles(lastTileIndex, terrainToolsUi.SelectedTile);
			}
		}
		// stop drawing when the left mouse button is released
		else if (@event.IsActionReleased("left_click"))
		{
			isDrawing = false;
			lastTileIndex = new Vector2I(-1, -1);
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
					newLayer.SetCell(new Vector2I(x, y), 0, Vector2I.Zero);
				}
			}

			// apply alpha shader
			ShaderMaterial material = new();
			material.Shader = alphaShader;
			material.SetShaderParameter("tile_texture", terrainToolsUi.SelectedTile.Texture);
			newLayer.Material = material;

			// add to scene and dictionary
			terrainGrids.AddChild(newLayer);
			displayLayersDict.Add(tileName, newLayer);
			return newLayer;
		}
		// otherwise get the existing layer for that tile
		return displayLayersDict[tileName];
	}

	// Draw using the brush size
	private void BrushDrawTiles(Vector2I tileIndex, Tile tileType)
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
		if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
		{
			return false;
		}
		return baseTiles[x, y] == tileType;
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

	private void OnBrushSizeChanged(bool valueChanged)
    {
        if (valueChanged)
		{
			brushSize = (int)brushSlider.Value;
			GD.Print($"Brush size changed to {brushSize}");
		}
    }



    // B. implement strokes (rather than just setting 1 tile)
    // C. implement erasing tiles
    // D. implement it as a command that gathers all drawn tiles
    // E. implement brush sizes (which will need to account for setting nonexistent tiles at the edge of the base terrain index)

}

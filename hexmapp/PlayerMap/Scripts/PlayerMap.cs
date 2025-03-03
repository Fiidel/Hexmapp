using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerMap : Node2D
{
	// general variables
	private TerrainToolsUi terrainToolsUi;
	private BaseMapData baseMapData;
	private TileMapLayer baseGridLayer;
	private TileMapLayer displayGridLayer;
	private Dictionary<string, TileMapLayer> displayLayersDict = new();
	private bool isTileDrawing;
	private Vector2I lastTileIndex = new Vector2I(-1, -1);
	private DrawTilesCommand currentDrawTilesCommand = null;
	private Control uiPanel;
	private Sprite2D mapAssetPreview;

	// parent nodes for other objects
	private Node terrainGrids;
	private Node2D mapAssets;

	// resources
	private Shader alphaShader = GD.Load<Shader>("res://PlayerMap/alpha_mask.gdshader");
	private NoiseTexture2D alphaNoiseTexture = GD.Load<NoiseTexture2D>("res://PlayerMap/player_map_noise.tres");


	public override void _Ready()
	{
		// get nodes and resources
		baseGridLayer = GetNode<TileMapLayer>("TerrainGrids/BaseTerrainGrid");
		displayGridLayer = GetNode<TileMapLayer>("TerrainGrids/DisplayTerrainOffsetGrid");
		terrainToolsUi = GetNode<TerrainToolsUi>("TerrainToolsUI");
		terrainGrids = GetNode<Node>("TerrainGrids");
		mapAssets = GetNode<Node2D>("MapAssets");
		uiPanel = GetNode<Control>("TerrainToolsUI/PanelContainer");
		mapAssetPreview = GetNode<Sprite2D>("%MapAssetPreview");

		// check for missing data and errors
		ValiadeLoadsAfterReady();

		// create base map
		baseMapData = new BaseMapData(200, 150);

		// register signals
		uiPanel.MouseEntered += OnUiPanelMouseEntered;
		uiPanel.MouseExited += OnUiPanelMouseExited;
	}


    public override void _ExitTree()
	{
		base._ExitTree();
		uiPanel.MouseEntered -= OnUiPanelMouseEntered;
		uiPanel.MouseExited -= OnUiPanelMouseExited;
	}


    private void ValiadeLoadsAfterReady()
    {
        if (baseGridLayer == null)
		{
			GD.Print("Base tile map layer not found.");
			return;
		}
		if (displayGridLayer == null)
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
		if (mapAssets == null)
		{
			GD.Print("Map assets node not found.");
			return;
		}
		if (alphaShader == null)
		{
			GD.Print("Alpha shader not found.");
			return;
		}
		if (alphaNoiseTexture == null)
		{
			GD.Print("Alpha noise texture not found.");
			return;
		}
		if (uiPanel == null)
		{
			GD.Print("UI panel not found.");
			return;
		}
    }


    public override void _UnhandledInput(InputEvent @event)
	{
		// start drawing tiles with the selected tile brush
		if (@event.IsActionPressed("left_click"))
		{
			if (terrainToolsUi.SelectedTool is Tile tile)
			{
				StartDrawingTiles(tile);
			}
			else if (terrainToolsUi.SelectedTool is MapAsset mapAsset)
			{
				var placeMapAssetCommand = new PlaceMapAssetCommand(mapAssets, mapAsset, GetGlobalMousePosition());
				placeMapAssetCommand.Execute();
			}
			else if (terrainToolsUi.SelectedTool is MapPin mapPin)
			{
				var placeMapPinCommand = new PlaceMapPinCommand(mapAssets, mapPin, GetGlobalMousePosition());
				placeMapPinCommand.Execute();
			}
		}
		// continous drawing while the left mouse button is down and moving
		else if (isTileDrawing && @event is InputEventMouseMotion)
		{
			if (terrainToolsUi.SelectedTool is Tile tile)
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


    public override void _Process(double delta)
    {
        if (mapAssetPreview.Visible)
		{
			mapAssetPreview.GlobalPosition = GetGlobalMousePosition();
		}
    }


    private void OnUiPanelMouseExited()
    {
		if (terrainToolsUi.SelectedTool is MapAsset mapAsset)
		{
			SetupMapAssetPreview(mapAsset.Texture);
			mapAssetPreview.ZIndex = 1;
		}
		else if (terrainToolsUi.SelectedTool is MapPin mapPin)
		{
			SetupMapAssetPreview(mapPin.Texture);
			mapAssetPreview.ZIndex = 2;
		}
    }


	private void SetupMapAssetPreview(Texture2D texture)
	{
		mapAssetPreview.Texture = texture;
		Input.MouseMode = Input.MouseModeEnum.Hidden;
		mapAssetPreview.Visible = true;
	}


    private void OnUiPanelMouseEntered()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
		mapAssetPreview.Visible = false;
    }


    private void StartDrawingTiles(Tile tile)
    {
        var clickedTile = baseGridLayer.LocalToMap(GetGlobalMousePosition());
		GetOrCreateTileMapLayer(tile.IdName);
		// for continous drawing
		isTileDrawing = true;
		lastTileIndex = clickedTile;

		// create a new command that accumulates drawn tiles
		currentDrawTilesCommand = new DrawTilesCommand(baseMapData, displayLayersDict, new List<Vector2I>(), terrainToolsUi.BrushSize, tile);
		// draw the first tile
		AddTileToCommandAndDraw(clickedTile, tile);
    }


	private void ContinueDrawingTiles(Tile tile)
    {
        var currentTile = baseGridLayer.LocalToMap(GetGlobalMousePosition());
		if (lastTileIndex != currentTile)
		{
			lastTileIndex = currentTile;
			AddTileToCommandAndDraw(currentTile, tile);
		}
    }


    private void StopDrawingTiles()
    {
        isTileDrawing = false;
		lastTileIndex = new Vector2I(-1, -1);

		// execute the whole tile draw command (in case of )
		currentDrawTilesCommand.Execute();
		currentDrawTilesCommand = null;
    }


    private void AddTileToCommandAndDraw(Vector2I tileIndex, Tile tile)
	{
		currentDrawTilesCommand.tileIndices.Add(tileIndex);
		currentDrawTilesCommand.BrushDrawTiles(tileIndex, tile);
	}


    // Get or create a tile map layer
    private TileMapLayer GetOrCreateTileMapLayer(string tileName)
	{
		// if the tile hasn't been used yet, create a new layer
		if (!displayLayersDict.ContainsKey(tileName) && terrainToolsUi.SelectedTool is Tile tile)
		{
			TileMapLayer newLayer = displayGridLayer.Duplicate() as TileMapLayer;
			newLayer.Name = $"{tileName}Layer";
			newLayer.SetMeta("Tile", tile);
			for (int x = 0; x <= baseMapData.mapWidth; x++) // <= ... to account for the offset to top left by half a tile
			{
				for (int y = 0; y <= baseMapData.mapHeight; y++)
				{
					newLayer.SetCell(new Vector2I(x, y), 0, Vector2I.Zero);
				}
			}

			// apply alpha shader
			ShaderMaterial material = new();
			material.Shader = alphaShader;
			material.SetShaderParameter("tile_texture", tile.Texture);
			material.SetShaderParameter("noise", alphaNoiseTexture);
			newLayer.Material = material;

			// add to scene and dictionary
			terrainGrids.AddChild(newLayer);
			displayLayersDict.Add(tileName, newLayer);
			return newLayer;
		}
		// otherwise get the existing layer for that tile
		return displayLayersDict[tileName];
	}
}

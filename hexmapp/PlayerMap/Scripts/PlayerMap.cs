using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerMap : Node2D
{
	// public variables
	public Vector2I lastTileIndex = new Vector2I(-1, -1);
	public TileMapDrawTool tileMapDrawTool { get; private set; }

	// general variables
	public TerrainToolsUi terrainToolsUi;
	private BaseMapData baseMapData;
	private TileMapLayer baseGridLayer;
	private TileMapLayer displayGridLayer;
	private Dictionary<string, TileMapLayer> displayLayersDict = new();
	private PlayerMapFSM playerMapFSM;
	private bool isTileDrawing;
	private bool isTileErasing;
	private Control uiPanel;
	private Sprite2D mapAssetPreview;
	private RectangleOutline objectSelectionOutline;

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

		// initialize
		baseMapData = new BaseMapData(200, 150);
		tileMapDrawTool = new TileMapDrawTool(baseMapData, displayLayersDict);
		playerMapFSM = new PlayerMapFSM(this);

		// register signals
		uiPanel.MouseEntered += OnUiPanelMouseEntered;
		uiPanel.MouseExited += OnUiPanelMouseExited;
		terrainToolsUi.SelectedModeChanged += OnSelectedModeChanged;
	}


    public override void _ExitTree()
	{
		base._ExitTree();
		uiPanel.MouseEntered -= OnUiPanelMouseEntered;
		uiPanel.MouseExited -= OnUiPanelMouseExited;
		terrainToolsUi.SelectedModeChanged -= OnSelectedModeChanged;
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
		playerMapFSM.currentState.ProcessInput(@event);
	}


    public override void _Process(double delta)
    {
        if (mapAssetPreview.Visible)
		{
			mapAssetPreview.GlobalPosition = GetGlobalMousePosition();
		}
    }


	private void OnSelectedModeChanged(PlayerMapModeEnum mode)
    {
        playerMapFSM.ChangeState(mode);
    }


	public Resource GetSelectedTool()
	{
		return terrainToolsUi.SelectedTool;
	}


	public int GetBrushSize()
	{
		return terrainToolsUi.BrushSize;
	}


	public void PlaceMapAsset(MapAsset mapAsset)
	{
		var placeMapAssetCommand = new PlaceMapAssetCommand(mapAssets, mapAsset, GetGlobalMousePosition());
		placeMapAssetCommand.Execute();
	}


	public void PlaceMapPin(MapPin mapPin)
	{
		var placeMapPinCommand = new PlaceMapPinCommand(mapAssets, mapPin, GetGlobalMousePosition());
		placeMapPinCommand.Execute();
	}


	public DrawTilesCommand CreateDrawTilesCommand(Tile tile)
	{
        GetOrCreateTileMapLayer(tile.IdName);
		return new DrawTilesCommand(baseMapData, displayLayersDict, new List<Vector2I>(), GetBrushSize(), tile);
	}


	public EraseTilesCommand CreateEraseTilesCommand()
	{
		return new EraseTilesCommand(baseMapData, displayLayersDict, new List<Vector2I>(), GetBrushSize());
	}


    private void OnUiPanelMouseExited()
    {
		if (GetSelectedTool() is MapAsset mapAsset)
		{
			SetupMapAssetPreview(mapAsset.Texture);
			mapAssetPreview.ZIndex = 1;
		}
		else if (GetSelectedTool() is MapPin mapPin)
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


	public Vector2I GetTileIndex()
	{
		return baseGridLayer.LocalToMap(GetGlobalMousePosition());
	}


	public Vector2I GetAndLogTileIndex()
	{
		var clickedTile = GetTileIndex();
		lastTileIndex = clickedTile;
		return clickedTile;
	}


    // Get or create a tile map layer
    private TileMapLayer GetOrCreateTileMapLayer(string tileName)
	{
		// if the tile hasn't been used yet, create a new layer
		if (!displayLayersDict.ContainsKey(tileName) && GetSelectedTool() is Tile tile)
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


	private void AddRectOutline(Node objectToOutline, Texture2D objectTexture)
	{
		objectSelectionOutline = new RectangleOutline(objectTexture);
		objectSelectionOutline.Reparent(objectToOutline);
	}


	private void RemoveRectOutline()
	{
		if (objectSelectionOutline != null)
		{
			objectSelectionOutline.QueueFree();
			objectSelectionOutline = null;
		}
	}
}

using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class PlayerMap : Node2D
{
	// public variables
	public Vector2I lastTileIndex = new Vector2I(-1, -1);
	public TileMapDrawTool tileMapDrawTool { get; private set; }

	// general variables
	public TerrainToolsUi terrainToolsUi;
	private BaseMapData baseMapData;
	private Area2D backgroundDetectArea;
	private TileMapLayer baseGridLayer;
	private TileMapLayer displayGridLayer;
	private Dictionary<string, TileMapLayer> displayLayersDict = new();
	private PlayerMapFSM playerMapFSM;
	private bool isTileDrawing;
	private bool isTileErasing;
	private Control uiPanel;
	private Sprite2D mapAssetPreview;

	// parent nodes for other objects
	private Node terrainGrids;
	public Node2D mapAssets;

	// resources
	private Shader alphaShader = GD.Load<Shader>("res://PlayerMap/alpha_mask.gdshader");
	private NoiseTexture2D alphaNoiseTexture = GD.Load<NoiseTexture2D>("res://PlayerMap/player_map_noise.tres");


	public override void _Ready()
	{
		// get nodes and resources
		backgroundDetectArea = GetNode<Area2D>("%BackgroundDetectLayer");
		baseGridLayer = GetNode<TileMapLayer>("TerrainGrids/BaseTerrainGrid");
		displayGridLayer = GetNode<TileMapLayer>("TerrainGrids/DisplayTerrainOffsetGrid");
		terrainToolsUi = GetNode<TerrainToolsUi>("TerrainToolsUI");
		terrainGrids = GetNode<Node>("TerrainGrids");
		mapAssets = GetNode<Node2D>("MapAssets");
		uiPanel = GetNode<Control>("TerrainToolsUI/PanelContainer");
		mapAssetPreview = GetNode<Sprite2D>("%MapAssetPreview");

		// check for missing data and errors
		ValidateLoadsAfterReady();

		// initialize
		InitializeMapBasedOnSize(200, 150);
		playerMapFSM = new PlayerMapFSM(this);

		// register signals
		uiPanel.MouseEntered += OnUiPanelMouseEntered;
		uiPanel.MouseExited += OnUiPanelMouseExited;
		terrainToolsUi.SelectedModeChanged += OnSelectedModeChanged;
		backgroundDetectArea.InputEvent += OnBackgroundInputEvent;
	}


    public override void _ExitTree()
	{
		base._ExitTree();
		uiPanel.MouseEntered -= OnUiPanelMouseEntered;
		uiPanel.MouseExited -= OnUiPanelMouseExited;
		terrainToolsUi.SelectedModeChanged -= OnSelectedModeChanged;
		backgroundDetectArea.InputEvent -= OnBackgroundInputEvent;
	}


	private void InitializeMapBasedOnSize(int width, int height)
	{
		baseMapData = new BaseMapData(width, height);
		tileMapDrawTool = new TileMapDrawTool(baseMapData, displayLayersDict);

		// set up background click detection
		var backgroundDetectCollider = backgroundDetectArea.GetNode<CollisionShape2D>("CollisionShape2D");
		var tileSize = displayGridLayer.TileSet.TileSize.X;
		var mapSize = new Vector2(baseMapData.mapWidth * tileSize, baseMapData.mapHeight * tileSize);
		backgroundDetectCollider.Shape = new RectangleShape2D{Size = mapSize};
		backgroundDetectCollider.Position = mapSize / 2;

		// draw map outline
		var mapOutline = new RectangleOutline(mapSize, 5, new Color(0.36f, 0.21f, 0.08f));
		mapOutline.GlobalPosition = mapSize / 2;
		AddChild(mapOutline);
	}


    private void ValidateLoadsAfterReady()
    {
		if (backgroundDetectArea == null)
		{
			GD.Print("Background detection area layer not found.");
			return;
		}
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


	private void OnSelectedModeChanged(MapModeEnum mode)
    {
        playerMapFSM.ChangeState(mode);
    }


	private void OnBackgroundInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
		if (playerMapFSM.currentState is PlayerSelectState && @event.IsActionPressed("left_click"))
		{
			SignalBus.Instance.EmitSignal(SignalBus.SignalName.ClickedPlayerMapBackground);
		}
    }


	public Resource GetSelectedTool()
	{
		return terrainToolsUi.SelectedTool;
	}


	public int GetBrushSize()
	{
		return terrainToolsUi.BrushSize;
	}


	public bool IsClickInMapBounds(Vector2 position)
	{
		var tileSize = displayGridLayer.TileSet.TileSize.X;
		var mapSize = new Vector2(baseMapData.mapWidth * tileSize, baseMapData.mapHeight * tileSize);
		return position.X >= 0 && position.Y >= 0 && position.X <= mapSize.X && position.Y <= mapSize.Y;
	}


	public DrawTilesCommand CreateDrawTilesCommand(Tile tile)
	{
        GetOrCreateTileMapLayer(tile);
		return new DrawTilesCommand(baseMapData, displayLayersDict, new List<Vector2I>(), GetBrushSize(), tile);
	}


	public EraseTilesCommand CreateEraseTilesCommand()
	{
		return new EraseTilesCommand(baseMapData, displayLayersDict, new List<Vector2I>(), GetBrushSize());
	}


	private bool IsMouseOverUi()
	{
		return uiPanel.GetGlobalRect().HasPoint(GetGlobalMousePosition());
	}


    private void OnUiPanelMouseExited()
    {
		if (!IsMouseOverUi() && playerMapFSM.currentState is PlayerDrawState)
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
		else
		{
			HideAssetPreview();
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
        HideAssetPreview();
    }


	private void HideAssetPreview()
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
    private TileMapLayer GetOrCreateTileMapLayer(Tile tile)
	{
		// if the tile hasn't been used yet, create a new layer
		if (!displayLayersDict.ContainsKey(tile.IdName))
		{
			TileMapLayer newLayer = displayGridLayer.Duplicate() as TileMapLayer;
			newLayer.Name = $"{tile.IdName}Layer";
			newLayer.SetMeta("Tile", tile);
			for (int x = 0; x <= baseMapData.mapWidth; x++) // <= (less or equal) ... to account for the offset to top left by half a tile
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
			displayLayersDict.Add(tile.IdName, newLayer);
			return newLayer;
		}
		// otherwise get the existing layer for that tile
		return displayLayersDict[tile.IdName];
	}

	public Godot.Collections.Dictionary<string, Variant> Save()
	{
		// save the width and height in size, and base tilemap (as number IDs; you can store an ID-for-texturename dict as well)
		var saveData = new Godot.Collections.Dictionary<string, Variant>();
		saveData["size"] = new Godot.Collections.Dictionary<string, int>
		{
			{"width", baseMapData.mapWidth},
			{"height", baseMapData.mapHeight}
		};

		var mapData = new Godot.Collections.Array();
		for (int y = 0; y < baseMapData.mapHeight; y++)
		{
			var row = new Godot.Collections.Array();
			for (int x = 0; x < baseMapData.mapWidth; x++)
			{
				if (baseMapData.baseTiles[x, y] != null)
				{
					row.Add(baseMapData.baseTiles[x, y].IdName);
				}
				else
				{
					row.Add("-");
				}
			}
			mapData.Add(row);
		}
		saveData["map"] = mapData;

		return saveData;
	}

	public void Load(Godot.Collections.Dictionary<string, Variant> data)
	{
		// load all Tile resources
		var tileResources = LoadAllTileResources();
		if (tileResources.Count == 0)
		{
			GD.Print("No tile resources found in TileResources folder.");
			return;
		}
		
		// load width and height
		var mapSize = (Godot.Collections.Dictionary<string, int>) data["size"];
		InitializeMapBasedOnSize(mapSize["width"], mapSize["height"]);

		// load and render base tilemap
		tileMapDrawTool = new TileMapDrawTool(baseMapData, displayLayersDict);
		var mapData = (Godot.Collections.Array) data["map"];
		for (int y = 0; y < baseMapData.mapHeight; y++)
		{
			var row = (Godot.Collections.Array) mapData[y];
			for (int x = 0; x < baseMapData.mapWidth; x++)
			{
				var tileName = (string) row[x];
				if (tileResources.ContainsKey(tileName))
				{
					var tile = tileResources[tileName];
					GetOrCreateTileMapLayer(tile);
					tileMapDrawTool.AddTile(new Vector2I(x, y), tile);
				}
			}
		}
	}

	private Godot.Collections.Dictionary<string, Tile> LoadAllTileResources()
	{
		var tileResources = new Godot.Collections.Dictionary<string, Tile>();
		var resourcePath = ProjectSettings.GlobalizePath("res://PlayerMap/Tiles/TileResources/");

		using var resourceDir = DirAccess.Open(resourcePath);
		if (resourceDir == null)
		{
			return tileResources;
		}
		foreach (var fileName in resourceDir.GetFiles())
		{
			try
			{
				var tile = ResourceLoader.Load<Tile>(Path.Combine(resourcePath, fileName));
				if (tile != null)
				{
					tileResources.Add(tile.IdName, tile);
				}
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error loading Tile resource {fileName}: {e.Message}");
			}
		}
		return tileResources;
	}
}

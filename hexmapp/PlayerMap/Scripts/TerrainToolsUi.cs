using Godot;
using Godot.Collections;

public partial class TerrainToolsUi : CanvasLayer
{
	public Resource SelectedTool;
	public int BrushSize = 1;
	[Export]
	private ButtonGroup buttonGroup;
	[Export]
	private Array<Tile> terrainTiles;
	[Export]
	private Array<MapAsset> mapAssets;
	[Export]
	private Array<MapPin> mapPins;
	private BaseButton selectedButton;
	private GridContainer terrainTypeGrid;
	private GridContainer mapAssetGrid;
	private GridContainer pinGrid;
	private TextureRect outline;
	[Export]
	private PackedScene terrainTileButton;
	private HSlider brushSlider;

	public override void _Ready()
	{
		// get nodes and resources
		outline = GetNode<TextureRect>("SelectionOutline");
		terrainTypeGrid = GetNode<GridContainer>("%TerrainTypeGrid");
		mapAssetGrid = GetNode<GridContainer>("%MapAssetGrid");
		pinGrid = GetNode<GridContainer>("%PinGrid");
		brushSlider = GetNode<HSlider>("%TerrainBrushSizeSlider");


		// check for missing data and errors
		if (buttonGroup == null)
		{
			GD.Print("No terrain tool button group assigned!");
			return;
		}
		if (terrainTiles == null || terrainTiles.Count == 0)
		{
			GD.Print("No terrain tool buttons loaded.");
			return;
		}
		if (terrainTileButton == null)
		{
			GD.Print("No terrain tool button scene loaded.");
			return;
		}
		if (outline == null)
		{
			GD.Print("Warning: Selection outline not found.");
			return;
		}
		if (terrainTypeGrid == null)
		{
			GD.Print("Warning: Terrain type grid not found.");
			return;
		}
		if (mapAssetGrid == null)
		{
			GD.Print("Warning: Map asset grid not found.");
			return;
		}
		if (pinGrid == null)
		{
			GD.Print("Warning: Pin grid not found.");
			return;
		}
		if (brushSlider == null)
		{
			GD.Print("Terrain brush slider not found.");
			return;
		}
		

		// load all Tile resources as UI buttons
		for (int i = 0; i < terrainTiles.Count; i++)
		{
			TextureButton button = (TextureButton)terrainTileButton.Instantiate();
			button.SetMeta("Tile", terrainTiles[i]);
			button.Name = terrainTiles[i].IdName;
			button.TextureNormal = terrainTiles[i].Texture;
			button.ToggleMode = true;
			button.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(button, TerrainToolTypeEnum.TILE)));
			button.Pressed += () => OnButtonPressed(button, TerrainToolTypeEnum.TILE);
			button.AddToGroup(buttonGroup.ResourceName);

			terrainTypeGrid.AddChild(button);
		}

		// load all MapAsset resources as UI buttons
		for (int i = 0; i < mapAssets.Count; i++)
		{
			TextureButton button = (TextureButton)terrainTileButton.Instantiate();
			button.SetMeta("MapAsset", mapAssets[i]);
			button.TextureNormal = mapAssets[i].Texture;
			button.ToggleMode = true;
			button.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(button, TerrainToolTypeEnum.MAP_ASSET)));
			button.AddToGroup(buttonGroup.ResourceName);

			mapAssetGrid.AddChild(button);
		}

		// load all MapPin resources as UI buttons
		for (int i = 0; i < mapPins.Count; i++)
		{
			TextureButton button = (TextureButton)terrainTileButton.Instantiate();
			button.SetMeta("MapPin", mapPins[i]);
			button.TextureNormal = mapPins[i].Texture;
			button.ToggleMode = true;
			button.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(button, TerrainToolTypeEnum.PIN)));
			button.AddToGroup(buttonGroup.ResourceName);

			pinGrid.AddChild(button);
		}


		// select the first button
		selectedButton = terrainTypeGrid.GetChild(0) as BaseButton;
		if (selectedButton == null)
		{
			GD.Print("Warning: Couldn't convert first child of terrain type grid to BaseButton class.");
		}
		else
		{
			// toggle
			selectedButton.ButtonPressed = true;
			// outline
			outline.Visible = true;
			MoveOutlineToButton(selectedButton);
			// public tile reference
			SelectedTool = (Tile)selectedButton.GetMeta("Tile");

			GD.Print($"Selected tile {selectedButton.Name}");
		}

		// connect signals
		brushSlider.DragEnded += OnBrushSizeChanged;

		// inicialize variables
		BrushSize = (int)brushSlider.Value;
	}
	
	public override void _ExitTree()
    {
        base._ExitTree();
		brushSlider.DragEnded -= OnBrushSizeChanged;
    }

	private void OnButtonPressed(BaseButton button, TerrainToolTypeEnum toolType)
	{
		MoveOutlineToButton(button);

		switch (toolType)
		{
			case TerrainToolTypeEnum.TILE:
				SelectedTool = (Tile)button.GetMeta("Tile");
				break;
			case TerrainToolTypeEnum.MAP_ASSET:
				SelectedTool = (MapAsset)button.GetMeta("MapAsset");
				break;
			case TerrainToolTypeEnum.PIN:
				SelectedTool = (MapPin)button.GetMeta("MapPin");
				break;
			default:
				GD.Print("Error: Unrecognized tool type selected with button.");
				break;
		}
	}

	private void MoveOutlineToButton(BaseButton button)
	{
		outline.Visible = true;
		outline.Reparent(button);
		outline.Position = (button.Size - outline.Size) / 2;
	}

	private void OnBrushSizeChanged(bool valueChanged)
    {
        if (valueChanged)
		{
			BrushSize = (int)brushSlider.Value;
			GD.Print($"Brush size changed to {BrushSize}");
		}
    }
}

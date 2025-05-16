using Godot;
using System;
using Godot.Collections;

public partial class HexToolsUi : CanvasLayer
{
    public HexAsset SelectedHexAsset;

    private BaseButton drawModeButton;
	private BaseButton eraseModeButton;
	private BaseButton selectModeButton;
    [Signal] public delegate void SelectedModeChangedEventHandler(MapModeEnum mode);
    private BaseButton addPartyTokenButton;
    [Signal] public delegate void AddPartyTokenButtonPressedEventHandler();
    private TextureRect outline;
    [Export] private PackedScene toolButtonScene;
    [Export] private ButtonGroup hexButtonGroup;
    [Export] private Array<HexAsset> hexColors;
    [Export] private Array<HexAsset> hexTerrain;
    [Export] private Array<HexAsset> hexIcons;
    private GridContainer hexColorGrid;
    private GridContainer terrainGrid;
    private GridContainer iconsGrid;
    private BaseButton selectedButton;

    public override void _Ready()
    {
        drawModeButton = GetNode<BaseButton>("%DrawButton");
		eraseModeButton = GetNode<BaseButton>("%EraseButton");
		selectModeButton = GetNode<BaseButton>("%SelectButton");
        addPartyTokenButton = GetNode<Button>("%AddPartyTokenButton");
        outline = GetNode<TextureRect>("SelectionOutline");
        hexColorGrid = GetNode<GridContainer>("%HexColorGrid");
        terrainGrid = GetNode<GridContainer>("%TerrainGrid");
        iconsGrid = GetNode<GridContainer>("%IconsGrid");

        for (int i = 0; i < hexColors.Count; i++)
        {
            var button = LoadCommonButtonData(hexColors[i].Texture);
            button.SetMeta("HexAsset", hexColors[i]);
            button.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(button)));
            hexColorGrid.AddChild(button);
        }

        for (int i = 0; i < hexTerrain.Count; i++)
        {
            var button = LoadCommonButtonData(hexTerrain[i].Texture);
            button.SetMeta("HexAsset", hexTerrain[i]);
            button.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(button)));
            terrainGrid.AddChild(button);
        }

        for (int i = 0; i < hexIcons.Count; i++)
        {
            var button = LoadCommonButtonData(hexIcons[i].Texture);
            button.SetMeta("HexAsset", hexIcons[i]);
            button.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(button)));
            iconsGrid.AddChild(button);
        }

        // select the first button
        selectedButton = hexColorGrid.GetChild(0) as BaseButton;
		if (selectedButton == null)
		{
			GD.Print("Warning: Couldn't convert first child of hex color grid to BaseButton class.");
		}
		else
		{
			selectedButton.ButtonPressed = true;
			MoveOutlineToButton(selectedButton);
            SelectedHexAsset = (HexAsset)selectedButton.GetMeta("HexAsset");
		}

        // connect signals
		drawModeButton.Pressed += () => EmitSignal(SignalName.SelectedModeChanged, (int)MapModeEnum.DRAW);
        eraseModeButton.Pressed += () => EmitSignal(SignalName.SelectedModeChanged, (int)MapModeEnum.ERASE);
        selectModeButton.Pressed += () => EmitSignal(SignalName.SelectedModeChanged, (int)MapModeEnum.SELECT);
        addPartyTokenButton.Pressed += () => EmitSignal(SignalName.AddPartyTokenButtonPressed);

		// initialize variables
		drawModeButton.ButtonPressed = true;
    }

    private TextureButton LoadCommonButtonData(Texture2D texture)
	{
		TextureButton button = (TextureButton)toolButtonScene.Instantiate();
		button.TextureNormal = texture;
		button.ToggleMode = true;
		button.AddToGroup(hexButtonGroup.ResourceName);

		return button;
	}

    private void OnButtonPressed(BaseButton button)
    {
        MoveOutlineToButton(button);
        SelectedHexAsset = (HexAsset)button.GetMeta("HexAsset");
    }

    private void MoveOutlineToButton(BaseButton button)
	{
		outline.Visible = true;
		outline.Reparent(button);
		outline.Position = (button.Size - outline.Size) / 2;
	}
}

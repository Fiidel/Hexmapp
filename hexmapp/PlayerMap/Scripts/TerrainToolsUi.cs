using Godot;
using Godot.Collections;

public partial class TerrainToolsUi : CanvasLayer
{
	[Export]
	private ButtonGroup buttonGroup;
	private Array<BaseButton> baseButtons;
	private BaseButton selectedButton;
	private TextureRect outline;

	public override void _Ready()
	{
		outline = GetNode<TextureRect>("SelectionOutline");
		if (outline == null)
		{
			GD.Print("Warning: Selection outline not found.");
			return;
		}

		if (buttonGroup == null)
		{
			GD.Print("No terrain tool button group assigned!");
			return;
		}

		baseButtons = buttonGroup.GetButtons();
		if (baseButtons.Count == 0)
		{
			GD.Print("No terrain tool buttons loaded.");
			return;
		}
		buttonGroup.Pressed += OnTerrainButtonPressed;

		// select the first button
		selectedButton = baseButtons[0];
		// toggle
		selectedButton.ButtonPressed = true;
		// outline
		outline.Visible = true;
		MoveOutlineToButton(selectedButton);

		GD.Print($"Selected tile {selectedButton.Name}");
	}

    private void OnTerrainButtonPressed(BaseButton button)
    {
        selectedButton = buttonGroup.GetPressedButton();
		MoveOutlineToButton(selectedButton);
		GD.Print($"Selected tile {selectedButton.Name}");
    }

	private void MoveOutlineToButton(BaseButton button)
	{
		outline.Reparent(button);
		outline.Position = (button.Size - outline.Size) / 2;
		// outline.Position = Vector2.Zero; // (0,0) local to the button == outline renders where the button renders
	}


	// Get debug print: "Selected tile {name of node}, of type {terrain tool type}"
	// 1. get the type of the button from metadata (should I create a new class - type enum + path to texture?)
	// 2. print the debug message
}

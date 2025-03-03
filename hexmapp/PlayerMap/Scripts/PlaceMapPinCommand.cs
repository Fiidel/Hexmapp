using Godot;
using System;

public class PlaceMapPinCommand : Command
{
    private Node2D assetContainer;
    private MapPin pinAsset;
    private Vector2 position;
    private PackedScene mapPinScene = GD.Load<PackedScene>("res://PlayerMap/map_pin.tscn");

    public PlaceMapPinCommand(Node2D assetContainer, MapPin pinAsset, Vector2 position)
    {
        this.assetContainer = assetContainer;
        this.pinAsset = pinAsset;
        this.position = position;
    }

    public override void Execute()
    {
        PlaceNewMapPin(pinAsset);
    }

    private void PlaceNewMapPin(MapPin mapPin)
    {
        if (mapPinScene == null)
        {
            GD.Print("Error: map_pin scene didn't load.");
            return;
        }
        Node2D newMapPin = mapPinScene.Instantiate() as Node2D;
		newMapPin.GlobalPosition = new Vector2(MathF.Truncate(position.X), MathF.Truncate(position.Y));
		newMapPin.ZIndex = 2;
		assetContainer.AddChild(newMapPin);
    }
}
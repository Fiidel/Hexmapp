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
        PlaceNewMapPin();
    }

    private void PlaceNewMapPin()
    {
        if (mapPinScene == null)
        {
            GD.Print("Error: map_pin scene didn't load.");
            return;
        }
        Node2D newMapPin = mapPinScene.Instantiate() as Node2D;

        // set texture
        Sprite2D pinTexture = newMapPin.GetNode<Sprite2D>("Texture");
		pinTexture.Texture = pinAsset.Texture;
		
        // set collision shape size
        CollisionShape2D collisionShape = newMapPin.GetNode<CollisionShape2D>("CollisionShape2D");
        var rectShape = new RectangleShape2D();
        rectShape.Size = pinTexture.Texture.GetSize();
        collisionShape.Shape = rectShape;

        // place
        newMapPin.GlobalPosition = new Vector2(MathF.Truncate(position.X), MathF.Truncate(position.Y));
		newMapPin.ZIndex = 2;
		assetContainer.AddChild(newMapPin);
    }
}
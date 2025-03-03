using Godot;
using System;

public class PlaceMapAssetCommand : Command
{
    private Node2D assetContainer;
    private MapAsset mapAsset;
    private Vector2 position;

    public PlaceMapAssetCommand(Node2D assetContainer, MapAsset mapAsset, Vector2 position)
    {
        this.assetContainer = assetContainer;
        this.mapAsset = mapAsset;
        this.position = position;
    }

    public override void Execute()
    {
        PlaceNewMapAsset();
    }

    private void PlaceNewMapAsset()
    {
        var newMapAsset = new Sprite2D();
		newMapAsset.Texture = mapAsset.Texture;
		newMapAsset.GlobalPosition = new Vector2(MathF.Truncate(position.X), MathF.Truncate(position.Y));
		newMapAsset.Offset = new Vector2(0, - newMapAsset.Texture.GetSize().Y / 2);
		newMapAsset.ZIndex = 1;
		assetContainer.AddChild(newMapAsset);
    }
}
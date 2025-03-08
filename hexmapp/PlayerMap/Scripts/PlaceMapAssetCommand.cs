using Godot;
using System;

public class PlaceMapAssetCommand : Command
{
    private Node2D assetContainer;
    private MapAsset mapAsset;
    private Vector2 position;
    private PackedScene mapAssetScene = GD.Load<PackedScene>("res://PlayerMap/map_asset.tscn");

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
        if (mapAssetScene == null)
        {
            GD.Print("Error: map_asset scene didn't load.");
            return;
        }
        Node2D newMapAsset = mapAssetScene.Instantiate() as Node2D;

        // set texture
        Sprite2D assetTexture = newMapAsset.GetNode<Sprite2D>("AssetTexture");
		assetTexture.Texture = mapAsset.Texture;
		
        // set collision shape size
        CollisionShape2D collisionShape = newMapAsset.GetNode<CollisionShape2D>("CollisionShape2D");
        var rectShape = new RectangleShape2D();
        rectShape.Size = assetTexture.Texture.GetSize();
        collisionShape.Shape = rectShape;

        // place
        newMapAsset.GlobalPosition = new Vector2(MathF.Truncate(position.X), MathF.Truncate(position.Y));
		newMapAsset.ZIndex = 1;
		assetContainer.AddChild(newMapAsset);
    }
}
using Godot;
using System;

[GlobalClass]
public partial class HexAsset : Resource
{
    [Export]
    public Texture2D Texture { get; set; }

    [Export]
    public int TilesetId { get; set; }

    [Export]
    public HexAssetTypeEnum AssetType { get; set; }
}

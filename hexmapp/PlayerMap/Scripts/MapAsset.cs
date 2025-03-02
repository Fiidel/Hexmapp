using Godot;

[GlobalClass]
public partial class MapAsset : Resource
{
    [Export]
    public Texture2D Texture { get; set; }
    private Vector2I mapPosition;
}
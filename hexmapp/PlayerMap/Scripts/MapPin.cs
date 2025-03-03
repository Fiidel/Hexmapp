using Godot;

[GlobalClass]
public partial class MapPin : Resource
{
    [Export]
    public Texture2D Texture { get; set; }
    [Export]
    public string Description { get; set; }
    private Vector2I mapPosition;
}
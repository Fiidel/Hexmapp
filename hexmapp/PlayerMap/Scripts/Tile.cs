using Godot;

[GlobalClass]
public partial class Tile : Resource
{
    [Export]
    public string IdName { get; set; }
    [Export]
    public Texture2D Texture { get; set; }
}
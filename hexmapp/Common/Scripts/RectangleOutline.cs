using Godot;

public partial class RectangleOutline : Node2D
{
    private Texture2D spriteToOutline;
    private float thickness;

    public RectangleOutline(Texture2D spriteToOutline, float thickness = 2f)
    {
        this.spriteToOutline = spriteToOutline;
        this.thickness = thickness;
    }


    public override void _Draw()
    {
        Vector2 spriteSize = spriteToOutline.GetSize();
        DrawRect(new Rect2(-spriteSize.X / 2 - thickness, 
                           -spriteSize.Y / 2 - thickness, 
                           spriteSize.X + thickness * 2, 
                           spriteSize.Y + thickness * 2), 
                           Colors.LightBlue, 
                           false, 
                           thickness);
    }
}
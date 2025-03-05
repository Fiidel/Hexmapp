using Godot;

public partial class RectangleOutline : Node2D
{
    private Texture2D spriteToOutline;

    public RectangleOutline(Texture2D spriteToOutline)
    {
        this.spriteToOutline = spriteToOutline;
    }


    public override void _Draw()
    {
        Vector2 spriteSize = spriteToOutline.GetSize();
        DrawRect(new Rect2(0, 0, spriteSize.X, spriteSize.Y), Colors.Azure, false, 1.0f);
    }
}
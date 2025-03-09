using Godot;

public partial class RectangleOutline : Node2D
{
    private Vector2 sizeToOutline;
    private float thickness;
    private Color color = new Color(0.361f, 0.929f, 0.961f);

    public RectangleOutline(Vector2 sizeToOutline, float thickness = 2f, Color? color = null)
    {
        this.sizeToOutline = sizeToOutline;
        this.thickness = thickness;
        if (color != null)
        {
            this.color = (Color)color;
        }
    }


    public override void _Draw()
    {
        DrawRect(new Rect2(-sizeToOutline.X / 2 - thickness, 
                           -sizeToOutline.Y / 2 - thickness, 
                           sizeToOutline.X + thickness * 2, 
                           sizeToOutline.Y + thickness * 2), 
                           color, 
                           false, 
                           thickness);
    }
}
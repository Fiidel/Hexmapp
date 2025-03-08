using Godot;

public class MoveMapObjectCommand : Command
{
    private Area2D mapObject;
    private Vector2 newPosition;

    public MoveMapObjectCommand(Area2D mapObject, Vector2 newPosition)
    {
        this.mapObject = mapObject;
        this.newPosition = newPosition;
    }


    public override void Execute()
    {
        mapObject.GlobalPosition = newPosition;
    }
}
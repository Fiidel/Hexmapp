using Godot;

public class DeleteMapObjectCommand : Command
{
    private Area2D mapObjectToDelete;

    public DeleteMapObjectCommand(Area2D mapObjectToDelete)
    {
        this.mapObjectToDelete = mapObjectToDelete;
    }

    public override void Execute()
    {
        mapObjectToDelete.QueueFree();
    }
}
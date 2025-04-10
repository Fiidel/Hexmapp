using Godot;

public interface IMapState
{
    public void ProcessInput(InputEvent @event);
    public void EnterState();
    public void ExitState();
}
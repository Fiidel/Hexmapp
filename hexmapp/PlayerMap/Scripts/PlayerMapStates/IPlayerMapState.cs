using Godot;

public interface IPlayerMapState
{
    public void ProcessInput(InputEvent @event);
    public void EnterState();
    public void ExitState();
}
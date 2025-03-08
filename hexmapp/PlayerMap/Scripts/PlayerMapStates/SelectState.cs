using Godot;

public class SelectState : IPlayerMapState
{
    private PlayerMap context;

    public SelectState(PlayerMap playerMap)
    {
        context = playerMap;
    }


    public void EnterState()
    {
        SignalBus.Instance.ClickedPlayerMapObject += OnMapObjectClicked;
    }


    public void ExitState()
    {
        SignalBus.Instance.ClickedPlayerMapObject -= OnMapObjectClicked;
    }


    public void ProcessInput(InputEvent @event)
    {
        // no input logic here, handled through signals in the map object script
    }


    private void OnMapObjectClicked(Area2D mapObject)
    {
        GD.Print($"Detected click on node {mapObject.Name} at position {mapObject.GlobalPosition}");
    }
}
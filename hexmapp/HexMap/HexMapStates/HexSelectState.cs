using Godot;

public class HexSelectState : IMapState
{
    private HexMap context;

    public HexSelectState(HexMap hexMap)
    {
        context = hexMap;
    }

    public void EnterState() {}

    public void ExitState() {}

    public void ProcessInput(InputEvent @event)
    {
        
    }
}
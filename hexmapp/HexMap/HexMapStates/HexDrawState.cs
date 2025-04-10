using Godot;

public class HexDrawState : IMapState
{
    private HexMap context;

    public HexDrawState(HexMap hexMap)
    {
        context = hexMap;
    }

    public void EnterState() {}

    public void ExitState() {}

    public void ProcessInput(InputEvent @event)
    {
        if (@event.IsActionPressed("left_click"))
        {
            var tileIndex = context.GetTileIndex();
            context.DrawHexAsset(tileIndex);
        }
    }
}
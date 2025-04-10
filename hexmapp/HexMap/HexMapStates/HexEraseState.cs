using Godot;

public class HexEraseState : IMapState
{
    private HexMap context;

    public HexEraseState(HexMap hexMap)
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
            context.EraseHexAsset(tileIndex);
        }
    }
}
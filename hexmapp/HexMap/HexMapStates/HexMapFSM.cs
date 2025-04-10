public class HexMapFSM
{
    public HexMap context;
    public IMapState currentState;
    private HexDrawState drawState;
    private HexEraseState eraseState;
    private HexSelectState selectState;
    
    public HexMapFSM(HexMap hexMap)
    {
        context = hexMap;
        drawState = new HexDrawState(context);
        eraseState = new HexEraseState(context);
        selectState = new HexSelectState(context);

        Init();
    }


    public void Init()
    {
        currentState = drawState;
        currentState.EnterState();
    }


    public void ChangeState(MapModeEnum mode)
    {
        switch (mode)
        {
            case MapModeEnum.DRAW:
                ExitSetEnterState(drawState);
				break;
			case MapModeEnum.ERASE:
                ExitSetEnterState(eraseState);
				break;
			case MapModeEnum.SELECT:
                ExitSetEnterState(selectState);
				break;
        }
    }


    public void ExitSetEnterState(IMapState newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}
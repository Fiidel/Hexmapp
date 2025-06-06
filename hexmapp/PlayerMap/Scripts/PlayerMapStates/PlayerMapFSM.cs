public class PlayerMapFSM
{
    public IMapState currentState;
    private PlayerMap context;
    private PlayerDrawState drawState;
    private PlayerEraseState eraseState;
    private PlayerSelectState selectState;

    public PlayerMapFSM(PlayerMap playerMap)
    {
        context = playerMap;
        drawState = new PlayerDrawState(context);
        eraseState = new PlayerEraseState(context);
        selectState = new PlayerSelectState(context);

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
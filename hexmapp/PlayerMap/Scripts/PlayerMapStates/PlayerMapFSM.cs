public class PlayerMapFSM
{
    public IPlayerMapState currentState;
    private PlayerMap context;
    private DrawState drawState;
    private EraseState eraseState;
    private SelectState selectState;

    public PlayerMapFSM(PlayerMap playerMap)
    {
        context = playerMap;
        drawState = new DrawState(context);
        eraseState = new EraseState(context);
        selectState = new SelectState(context);

        Init();
    }


    public void Init()
    {
        currentState = drawState;
        currentState.EnterState();
    }


    public void ChangeState(PlayerMapModeEnum mode)
    {
        switch (mode)
        {
            case PlayerMapModeEnum.DRAW:
                ExitSetEnterState(drawState);
				break;
			case PlayerMapModeEnum.ERASE:
                ExitSetEnterState(eraseState);
				break;
			case PlayerMapModeEnum.SELECT:
                ExitSetEnterState(selectState);
				break;
        }
    }


    public void ExitSetEnterState(IPlayerMapState newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}
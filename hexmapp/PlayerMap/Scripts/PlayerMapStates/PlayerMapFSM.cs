public class PlayerMapFSM
{
    public IPlayerMapState currentState;
    private PlayerMap context;
    private IPlayerMapState drawState;
    private IPlayerMapState eraseState;

    public PlayerMapFSM(PlayerMap playerMap)
    {
        context = playerMap;
        drawState = new DrawState(context);
        eraseState = new EraseState(context);

        Init();
    }


    public void Init()
    {
        ChangeState(PlayerMapModeEnum.DRAW);
    }


    public void ChangeState(PlayerMapModeEnum mode)
    {
        switch (mode)
        {
            case PlayerMapModeEnum.DRAW:
				currentState = drawState;
				break;
			case PlayerMapModeEnum.ERASE:
                currentState = eraseState;
				break;
			case PlayerMapModeEnum.SELECT:
				break;
        }
    }
}
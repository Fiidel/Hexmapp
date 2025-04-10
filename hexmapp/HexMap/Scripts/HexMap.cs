using Godot;
using System;

public partial class HexMap : Node2D
{
    private HexToolsUi hexToolsUi;
    private HexMapFSM hexMapFSM;
    private TileMapLayer hexGrid;
    private TileMapLayer terrainGrid;
    private TileMapLayer iconsGrid;


    public override void _Ready()
    {
        hexToolsUi = GetNode<HexToolsUi>("HexToolsUI");
        hexToolsUi.SelectedModeChanged += OnSelectedModeChangedd;
        hexMapFSM = new HexMapFSM(this);

        hexGrid = GetNode<TileMapLayer>("%HexGrid");
        terrainGrid = GetNode<TileMapLayer>("%TerrainGrid");
        iconsGrid = GetNode<TileMapLayer>("%IconsGrid");
    }

    public override void _UnhandledInput(InputEvent @event)
	{
		hexMapFSM.currentState.ProcessInput(@event);
	}

    private void OnSelectedModeChangedd(MapModeEnum mode)
    {
        hexMapFSM.ChangeState(mode);
    }

    public Vector2I GetTileIndex()
    {
        return hexGrid.LocalToMap(GetGlobalMousePosition());
    }

    public void DrawHexAsset(Vector2I tileIndex)
    {
        var asset = hexToolsUi.SelectedHexAsset;
        switch (asset.AssetType)
        {
            case HexAssetTypeEnum.COLOR:
                hexGrid.SetCell(tileIndex, asset.TilesetId, Vector2I.Zero);
                break;
            case HexAssetTypeEnum.TERRAIN:
                terrainGrid.SetCell(tileIndex, asset.TilesetId, Vector2I.Zero);
                break;
            case HexAssetTypeEnum.ICON:
                iconsGrid.SetCell(tileIndex, asset.TilesetId, Vector2I.Zero);
                break;
        }
    }

    public void EraseHexAsset(Vector2I tileIndex)
    {
        if (iconsGrid.GetCellTileData(tileIndex) != null)
        {
            iconsGrid.EraseCell(tileIndex);
        }
        else if (terrainGrid.GetCellTileData(tileIndex) != null)
        {
            terrainGrid.EraseCell(tileIndex);
        }
        else if (hexGrid.GetCellTileData(tileIndex) != null)
        {
            hexGrid.EraseCell(tileIndex);
        }
    }
}

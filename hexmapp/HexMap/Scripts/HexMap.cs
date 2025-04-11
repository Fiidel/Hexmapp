using Godot;
using System;
using System.Collections.Generic;

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

    public Godot.Collections.Dictionary<string, Variant> Save()
    {
        var hexGridDictionary = GetTilemaplayerDictionary(hexGrid);
        var terrainGridDictionary = GetTilemaplayerDictionary(terrainGrid);
        var iconsGridDictionary = GetTilemaplayerDictionary(iconsGrid);

        var allTiles = new Godot.Collections.Dictionary<string, Variant>
        {
            {"hexGrid", hexGridDictionary},
            {"terrainGrid", terrainGridDictionary},
            {"iconsGrid", iconsGridDictionary}
        };

        GD.Print(allTiles);
        
        return allTiles;
    }

    private Godot.Collections.Dictionary<Vector2I, int> GetTilemaplayerDictionary(TileMapLayer tileMapLayer)
    {
        var dictionary = new Godot.Collections.Dictionary<Vector2I, int>();
        var usedCells = tileMapLayer.GetUsedCells();
        foreach (var tile in usedCells)
        {
            dictionary.Add(tile, tileMapLayer.GetCellSourceId(tile));
        }
        return dictionary;
    }

    public void Load(Godot.Collections.Dictionary<string, Variant> data)
    {
        try
        {
            var hexGridDictionary = (Godot.Collections.Dictionary<Vector2I, int>) data["hexGrid"];
            var terrainGridDictionary = (Godot.Collections.Dictionary<Vector2I, int>) data["terrainGrid"];
            var iconsGridDictionary = (Godot.Collections.Dictionary<Vector2I, int>) data["iconsGrid"];

            foreach (var tile in hexGridDictionary)
            {
                hexGrid.SetCell(tile.Key, tile.Value, Vector2I.Zero);
            }
            foreach (var tile in terrainGridDictionary)
            {
                terrainGrid.SetCell(tile.Key, tile.Value, Vector2I.Zero);
            }
            foreach (var tile in iconsGridDictionary)
            {
                iconsGrid.SetCell(tile.Key, tile.Value, Vector2I.Zero);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr(e.Message);
            return;
        }
    }
}

using Godot;

[GlobalClass]
public partial class BaseMapData : Resource
{
    public int mapWidth;
    public int mapHeight;
    public Tile[,] baseTiles;
    
    public BaseMapData(int mapWidth, int mapHeight)
    {
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        baseTiles = new Tile[mapWidth, mapHeight];
    }
}
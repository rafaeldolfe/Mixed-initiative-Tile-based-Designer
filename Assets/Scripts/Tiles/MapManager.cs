using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

public class TileInformation
{
    public float walkability;
    public SimpleTileType tileType;
    public Vector3Int position;
    public List<SpecialFunctionality> specialFunctionalities;
}
public class MapManager : MonoBehaviour
{
    public Tilemap tilemap;
    public Dictionary<Vector3Int, TileInformation> tilesDict = new Dictionary<Vector3Int, TileInformation>();
    public TileInformation entrance;
    public TileInformation goal;

    public void AddTile(Vector3Int position, ScriptableTile tile)
    {
        tilesDict[position] = new TileInformation { walkability = tile.walkability, tileType = tile.tileType, position = position };
    }
    public void AddTile(Vector3Int position, OverflowTile tile)
    {
        for (int i = 0; i < tile.width; i++)
        {
            for (int j = 0; j < tile.height; j++)
            {
                Vector3Int finalPos = position + new Vector3Int(i, j, 0);
                tilesDict[finalPos] = new TileInformation { tileType = tile.tileType, walkability = tile.walkability, position = finalPos };
            }
        }
    }
    public void SetFunctionalities(Vector3Int position, List<SpecialFunctionality> specialFunctionalities)
    {
        tilesDict[position].specialFunctionalities = specialFunctionalities;
    }
}
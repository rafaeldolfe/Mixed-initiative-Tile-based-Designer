using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    public RoomTemplate template;
    public HashSet<Vector3Int> finalTilePositions = new HashSet<Vector3Int>();
    //public Dictionary<Vector3Int, Tile> finalTiles = new Dictionary<Vector3Int, Tile>();
}

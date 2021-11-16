using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class RoomTemplate
{
    public RoomType type;
    public int width;
    public int height;
    public List<Direction> connections = new List<Direction>();
    public Dictionary<Vector3Int, Tile> tilesDict;
}

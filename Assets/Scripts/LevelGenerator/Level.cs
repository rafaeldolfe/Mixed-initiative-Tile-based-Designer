using ShapeGrammar;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level
{
    public SpaceState spaceState;
    public List<Room> rooms = new List<Room>();
    public Vector3Int entrance;
    public Vector3Int goal;
    public HashSet<Vector3Int> tilePositions = new HashSet<Vector3Int>();
    //public Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();
    public Dictionary<string, List<Vector3Int>> guidToKeys = new Dictionary<string, List<Vector3Int>>();
    public Dictionary<string, List<Vector3Int>> guidToLock = new Dictionary<string, List<Vector3Int>>();
    public Dictionary<string, List<Vector3Int>> guidToKeyMultis = new Dictionary<string, List<Vector3Int>>();
    public Dictionary<string, List<Vector3Int>> guidToLockMulti = new Dictionary<string, List<Vector3Int>>();
    public Dictionary<Vector3Int, string> tilePosToGuid = new Dictionary<Vector3Int, string>();
    public Dictionary<Vector3Int, Tile> tilePosToTile = new Dictionary<Vector3Int, Tile>();
    public Dictionary<Vector3Int, Cell> tilePosToCell = new Dictionary<Vector3Int, Cell>();
}

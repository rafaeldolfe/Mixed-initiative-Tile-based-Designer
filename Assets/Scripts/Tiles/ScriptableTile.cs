using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

public enum SimpleTileType
{
    Empty,
    Wall,
    Spike,
    Gap,
    Key,
    Lock,
    KeyMulti,
    LockMulti,
    Monster,
    Treasure,
    Entrance,
    Goal,
}
[CreateAssetMenu]
public class ScriptableTile : Tile
{
    //private MapManager mm;
    public float walkability = 1;
    public SimpleTileType tileType;
    public SpecialNodeType specialNodeType;

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (!Application.isPlaying)
        {
            return base.StartUp(position, tilemap, go);
        }

        return base.StartUp(position, tilemap, go);
    }
}

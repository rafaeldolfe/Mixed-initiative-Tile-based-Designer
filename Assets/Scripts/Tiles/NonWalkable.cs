using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

[CreateAssetMenu]
public class Wall : Tile, ILifecycleTile
{
    public MapManager mm;
    public string type;

    public void TileAwake(Vector3Int position, Tilemap tilemap)
    {
    }

    public void TileStart(Vector3Int position, Tilemap tilemap)
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (mm = FindObjectOfType(typeof(MapManager)) as MapManager),
            };
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
    }
}
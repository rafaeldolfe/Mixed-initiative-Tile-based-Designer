using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

[CreateAssetMenu]
public class OverflowTile : ScriptableTile
{
    public int width;
    public int height;
    public ChildTile child;

    [ShowIf("@width != height")]
    [ValidateInput("@width != height && invertedVersion == null", defaultMessage: "Non-square overflow tile must have inverted version")]
    public OverflowTile invertedVersion;

    private void OnValidate()
    {
        if (width == height)
        {
            invertedVersion = null;
        }
    }

    public override bool StartUp(Vector3Int position, ITilemap readOnlyTilemap, GameObject go)
    {
        if (!Application.isPlaying)
        {
            return base.StartUp(position, readOnlyTilemap, go);
        }
        //Tilemap tilemap = readOnlyTilemap.GetComponent<Tilemap>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (child != null)
                {
                    SetTileDelayManager.Instance.Enqueue(position + new Vector3Int(i, j, 0), child);
                }
                //tilemap.SetTile(position + new Vector3Int(i, j, 0), child);
            }
        }

        return base.StartUp(position, readOnlyTilemap, go);
    }
}

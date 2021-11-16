using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class ProbabilisticTile : Tile
{
    public List<Tile> possibleTiles;

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (!Application.isPlaying)
        {
            return true;
        }
        SetTileDelayManager.Instance.Enqueue(position, possibleTiles.PickRandom());
        //tilemap.GetComponent<Tilemap>().SetTile(position, possibleTiles.PickRandom());

        //Debug.Log(go);
        //instance.sprite = possibleTiles.PickRandom();
        //tilemap.RefreshTile(position);
        return base.StartUp(position, tilemap, go);
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        if (!Application.isPlaying)
        {
            base.RefreshTile(position, tilemap);
            return;
        }
        SetTileDelayManager.Instance.Enqueue(position, possibleTiles.PickRandom());
        //tilemap.GetComponent<Tilemap>().SetTile(position, possibleTiles.PickRandom());
        base.RefreshTile(position, tilemap);
    }
}

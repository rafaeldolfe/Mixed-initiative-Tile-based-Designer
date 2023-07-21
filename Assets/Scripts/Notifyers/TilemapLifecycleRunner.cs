using Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

public class TilemapLifecycleRunner : MonoBehaviour
{
    private Tilemap tilemap;

    private void Awake()
    {
        this.tilemap = this.GetComponent<Tilemap>();
        if (this.tilemap == null)
        {
            throw ProgramUtils.MissingComponentException(typeof(Tilemap));
        }

        foreach (Vector3Int position in this.tilemap.cellBounds.allPositionsWithin)
        {
            if (this.tilemap.GetTile(position) is ILifecycleTile tile)
            {
                tile.TileAwake(position, this.tilemap);
            }
        }
    }

    private void Start()
    {
        foreach (Vector3Int position in this.tilemap.cellBounds.allPositionsWithin)
        {
            if (this.tilemap.GetTile(position) is ILifecycleTile tile)
            {
                tile.TileStart(position, this.tilemap);
            }
        }
    }
}
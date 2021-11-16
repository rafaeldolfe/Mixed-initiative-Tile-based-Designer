using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

public class TilemapLifecycleRunner : MonoBehaviour
{
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            throw ProgramUtils.MissingComponentException(typeof(Tilemap));
        }

        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.GetTile(position) is ILifecycleTile tile)
            {
                tile.TileAwake(position, tilemap);
            }
        }
    }

    private void Start()
    {
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.GetTile(position) is ILifecycleTile tile)
            {
                tile.TileStart(position, tilemap);
            }
        }
    }
}
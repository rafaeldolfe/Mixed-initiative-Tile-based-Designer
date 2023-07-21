namespace Tiles
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class TileInformation
    {
        public Vector3Int position;
        public List<SpecialFunctionality> specialFunctionalities;
        public SimpleTileType tileType;
        public float walkability;
    }

    public class MapManager : MonoBehaviour
    {
        public Tilemap tilemap;
        public TileInformation entrance;
        public TileInformation goal;
        public Dictionary<Vector3Int, TileInformation> tilesDict = new();

        public void AddTile(Vector3Int position, ScriptableTile tile)
        {
            this.tilesDict[position] = new TileInformation
            {
                walkability = tile.walkability, tileType = tile.tileType, position = position
            };
        }

        public void AddTile(Vector3Int position, OverflowTile tile)
        {
            for (int i = 0; i < tile.width; i++)
            for (int j = 0; j < tile.height; j++)
            {
                Vector3Int finalPos = position + new Vector3Int(i, j, 0);
                this.tilesDict[finalPos] = new TileInformation
                {
                    tileType = tile.tileType, walkability = tile.walkability, position = finalPos
                };
            }
        }

        public void SetFunctionalities(Vector3Int position, List<SpecialFunctionality> specialFunctionalities)
        {
            this.tilesDict[position].specialFunctionalities = specialFunctionalities;
        }
    }
}
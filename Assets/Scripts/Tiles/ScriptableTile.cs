namespace Tiles
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    [CreateAssetMenu(menuName = "EnvironmentTiles/Create RandomizerTile", fileName = "RandomizerTile",
        order = 0)]
    public class ScriptableTile : Tile, ILifecycleTile
    {
        public SimpleTileType tileType;
        public SpecialNodeType specialNodeType;
        public float walkability;
        public Vector3 offset;
        public List<GameObject> prefabs;
        public IntFloatDictionary numberToLikelihood;

        public void TileAwake(Vector3Int position, Tilemap tilemap)
        {
        }

        public void TileStart(Vector3Int position, Tilemap tilemap)
        {
        }

        private GameObject GetRandomPrefab(GameObject instancedGameObject)
        {
            GameObject clone = Instantiate(this.prefabs[Random.Range(0, this.prefabs.Count)],
                instancedGameObject.transform);
            return clone;
        }

        public override bool StartUp(Vector3Int position, ITilemap iTilemap, GameObject go)
        {
            if (go == null)
            {
                return base.StartUp(position, iTilemap, go);
            }

            long hash = position.x;
            hash = hash + 0xabcd1234 + (hash << 15);
            hash = (hash + 0x0987efab) ^ (hash >> 11);
            hash ^= position.y;
            hash = hash + 0x46ac12fd + (hash << 7);
            hash = (hash + 0xbe9730af) ^ (hash << 11);
            Random.State oldState = Random.state;
            Random.InitState((int)hash);

            if (this.prefabs.Count == 0)
            {
                return base.StartUp(position, iTilemap, go);
            }

            int n = WeightedRandomizer.From(this.numberToLikelihood).TakeOne();

            if (n == 0)
            {
                return base.StartUp(position, iTilemap, go);
            }

            GameObject clone1 = this.GetRandomPrefab(go);
            clone1.transform.position = new Vector3(position.x, position.y, position.z) + this.offset;

            Random.state = oldState;

            return base.StartUp(position, iTilemap, go);
        }
    }
}
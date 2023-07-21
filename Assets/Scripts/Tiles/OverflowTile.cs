namespace Tiles
{
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    [CreateAssetMenu]
    public class OverflowTile : ScriptableTile
    {
        public int width;
        public int height;
        public ChildTile child;

        [ShowIf("@width != height")]
        [ValidateInput("@width != height && invertedVersion == null",
            "Non-square overflow tile must have inverted version")]
        public OverflowTile invertedVersion;

        private void OnValidate()
        {
            if (this.width == this.height)
            {
                this.invertedVersion = null;
            }
        }

        public override bool StartUp(Vector3Int position, ITilemap readOnlyTilemap, GameObject go)
        {
            if (!Application.isPlaying)
            {
                return base.StartUp(position, readOnlyTilemap, go);
            }

            //Tilemap tilemap = readOnlyTilemap.GetComponent<Tilemap>();
            for (int i = 0; i < this.width; i++)
            for (int j = 0; j < this.height; j++)
            {
                if (this.child != null)
                {
                    SetTileDelayManager.Instance.Enqueue(position + new Vector3Int(i, j, 0), this.child);
                }
            }

            //tilemap.SetTile(position + new Vector3Int(i, j, 0), child);
            return base.StartUp(position, readOnlyTilemap, go);
        }
    }
}
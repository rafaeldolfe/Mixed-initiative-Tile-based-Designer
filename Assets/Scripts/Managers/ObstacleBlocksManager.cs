using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Tiles;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObstacleBlocksManager : MonoBehaviour
{
    private const int BORDER_SIZE = 1;
    public bool areInterestBlocks;
    public int sceneWidth;
    public int sceneHeight;
    public int obstacleBlockWidth;
    public int obstacleBlockHeight;
    public Tilemap tilemap;
    public Tile border;
    public Tile emptyTile;
    public int spacing;


    // Start is called before the first frame update
    private void Awake()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    [Button]
    public void GenerateBorders()
    {
        for (int i = 0; i < this.sceneWidth; i++)
        {
            int offsetX = i * ((BORDER_SIZE * 2) + this.obstacleBlockWidth + this.spacing);
            for (int j = 0; j < this.sceneHeight; j++)
            {
                int offsetY = j * ((BORDER_SIZE * 2) + this.obstacleBlockHeight + this.spacing);

                for (int x = -1; x < this.obstacleBlockWidth + 1; x++)
                {
                    for (int y = -1; y < this.obstacleBlockHeight + 1; y++)
                    {
                        if (x == -1 || y == -1 || x == this.obstacleBlockWidth || y == this.obstacleBlockHeight)
                        {
                            this.tilemap.SetTile(new Vector3Int(x + offsetX, y + offsetY, 0), this.border);
                        }
                        else if (!this.tilemap.HasTile(new Vector3Int(x + offsetX, y + offsetY, 0)))
                        {
                            this.tilemap.SetTile(new Vector3Int(x + offsetX, y + offsetY, 0), this.emptyTile);
                        }
                    }
                }
            }
        }
    }

    private string GetFolder()
    {
        return $"{Constants.OBSTACLE_BLOCK_FOLDER}/{this.obstacleBlockWidth}x{this.obstacleBlockHeight}";
    }

    [Button]
    public void SaveBlocks()
    {
        for (int i = 0; i < this.sceneWidth; i++)
        {
            int offsetX = i * ((BORDER_SIZE * 2) + this.obstacleBlockWidth + this.spacing);
            for (int j = 0; j < this.sceneHeight; j++)
            {
                int offsetY = j * ((BORDER_SIZE * 2) + this.obstacleBlockHeight + this.spacing);

                List<Tile> block = new();

                for (int x = 0; x < this.obstacleBlockWidth; x++)
                {
                    for (int y = 0; y < this.obstacleBlockHeight; y++)
                    {
                        block.Add(this.tilemap.GetTile(new Vector3Int(x + offsetX, y + offsetY, 0)) as Tile);
                    }
                }

                ObstacleBlock asset = ScriptableObject.CreateInstance<ObstacleBlock>();

                asset.width = this.obstacleBlockWidth;
                asset.height = this.obstacleBlockHeight;
                asset.tiles = block;

                this.CreateAssetInFolder(asset, $"{this.GetFolder()}", $"Block{j + (i * this.sceneWidth)}");
                AssetDatabase.SaveAssets();
            }
        }
    }

    public void MoveAssetIntoFolderDangerously(string oldPath, string newPath)
    {
        string[] pathSegments = newPath.Split(new[] {'/'});
        string accumulatedpath = Application.dataPath;
        foreach (string segment in pathSegments)
        {
            if (segment == "Assets" || segment.Contains(".asset"))
            {
                continue;
            }

            if (!Directory.Exists($"{accumulatedpath}/{segment}"))
            {
                string unityPath = accumulatedpath.Substring(accumulatedpath.IndexOf("Assets"));
                AssetDatabase.CreateFolder(unityPath, segment);
            }

            accumulatedpath += "/" + segment;
        }

        if (File.Exists(newPath))
        {
            AssetDatabase.DeleteAsset(newPath);
        }

        AssetDatabase.MoveAsset(oldPath, newPath);
    }

    public void CreateAssetInFolder(Object newAsset, string Folder, string AssetName)
    {
        string[] pathSegments = Folder.Split(new[] {'/'});
        string accumulatedpath = Application.dataPath;
        foreach (string segment in pathSegments)
        {
            if (segment == "Assets")
            {
                continue;
            }

            if (!Directory.Exists($"{accumulatedpath}/{segment}"))
            {
                string unityPath = accumulatedpath.Substring(accumulatedpath.IndexOf("Assets"));
                AssetDatabase.CreateFolder(unityPath, segment);
            }

            accumulatedpath += "/" + segment;
        }

        AssetDatabase.CreateAsset(newAsset, Folder + "/" + AssetName + ".asset");
    }
}
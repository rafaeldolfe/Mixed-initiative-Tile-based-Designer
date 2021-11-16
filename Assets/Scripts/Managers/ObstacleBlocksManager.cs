using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObstacleBlocksManager : MonoBehaviour
{
    public bool areInterestBlocks;
    public int sceneWidth;
    public int sceneHeight;
    public int obstacleBlockWidth;
    public int obstacleBlockHeight;
    public Tilemap tilemap;
    public Tile border;
    public Tile emptyTile;
    public int spacing;
    private const int BORDER_SIZE = 1;


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
        for (int i = 0; i < sceneWidth; i++)
        {
            int offsetX = i * (BORDER_SIZE * 2 + obstacleBlockWidth + spacing);
            for (int j = 0; j < sceneHeight; j++)
            {
                int offsetY = j * (BORDER_SIZE * 2 + obstacleBlockHeight + spacing);

                for (int x = -1; x < obstacleBlockWidth + 1; x++)
                {
                    for (int y = -1; y < obstacleBlockHeight + 1; y++)
                    {
                        if (x == -1 || y == -1 || x == obstacleBlockWidth || y == obstacleBlockHeight)
                        {
                            tilemap.SetTile(new Vector3Int(x + offsetX, y + offsetY, 0), border);
                        }
                        else if (!tilemap.HasTile(new Vector3Int(x + offsetX, y + offsetY, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x + offsetX, y + offsetY, 0), emptyTile);
                        }
                    }
                }
            }
        }
    }
    private string GetFolder()
    {
        return $"{Constants.OBSTACLE_BLOCK_FOLDER}/{obstacleBlockWidth}x{obstacleBlockHeight}";
    }
    [Button]
    public void SaveBlocks()
    {
        for (int i = 0; i < sceneWidth; i++)
        {
            int offsetX = i * (BORDER_SIZE * 2 + obstacleBlockWidth + spacing);
            for (int j = 0; j < sceneHeight; j++)
            {
                int offsetY = j * (BORDER_SIZE * 2 + obstacleBlockHeight + spacing);

                List<Tile> block = new List<Tile>();

                for (int x = 0; x < obstacleBlockWidth; x++)
                {
                    for (int y = 0; y < obstacleBlockHeight; y++)
                    {
                        block.Add(tilemap.GetTile(new Vector3Int(x + offsetX, y + offsetY, 0)) as Tile);
                    }
                }

                ObstacleBlock asset = ScriptableObject.CreateInstance<ObstacleBlock>();

                asset.width = obstacleBlockWidth;
                asset.height = obstacleBlockHeight;
                asset.tiles = block;

                CreateAssetInFolder(asset, $"{GetFolder()}", $"Block{j + i * sceneWidth}");
                AssetDatabase.SaveAssets();
            }
        }
    }

    public void MoveAssetIntoFolderDangerously(string oldPath, string newPath)
    {
        string[] pathSegments = newPath.Split(new char[] { '/' });
        string accumulatedpath = Application.dataPath;
        foreach (string segment in pathSegments)
        {
            if (segment == "Assets" || segment.Contains(".asset"))
            {
                continue;
            }
            if (!System.IO.Directory.Exists($"{accumulatedpath}/{segment}"))
            {
                string unityPath = accumulatedpath.Substring(accumulatedpath.IndexOf("Assets"));
                AssetDatabase.CreateFolder(unityPath, segment);
            }
            accumulatedpath += "/" + segment;
        }

        if (System.IO.File.Exists(newPath))
        {
            AssetDatabase.DeleteAsset(newPath);
        }

        AssetDatabase.MoveAsset(oldPath, newPath);
    }
    public void CreateAssetInFolder(Object newAsset, string Folder, string AssetName)
    {
        string[] pathSegments = Folder.Split(new char[] { '/' });
        string accumulatedpath = Application.dataPath;
        foreach (string segment in pathSegments)
        {
            if (segment == "Assets")
            {
                continue;
            }
            if (!System.IO.Directory.Exists($"{accumulatedpath}/{segment}"))
            {
                string unityPath = accumulatedpath.Substring(accumulatedpath.IndexOf("Assets"));
                AssetDatabase.CreateFolder(unityPath, segment);
            }
            accumulatedpath += "/" + segment;
        }

        AssetDatabase.CreateAsset(newAsset, Folder + "/" + AssetName + ".asset");
    }
}

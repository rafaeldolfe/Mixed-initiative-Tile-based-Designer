using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ObstacleType
{
    Obstacle,
    Interest
}

[CreateAssetMenu]
public class ObstacleBlockTile : OverflowTile, ILifecycleTile
{
    public bool RotatedVersion;
    public ObstacleType type;
    public List<ObstacleBlock> possibleBlocks;
    public ObstacleBlock testBlock;
    public GameObject prefabTest;

    public void TileAwake(Vector3Int position, Tilemap tilemap)
    {
    }

    public void TileStart(Vector3Int position, Tilemap tilemap)
    {
    }

    private void LoadBlocks(Vector3Int position, Tilemap tilemap)
    {
        List<ObstacleBlock> blocks = new List<ObstacleBlock>();

        string absolutePath = Application.dataPath +
            $"{Constants.OBSTACLE_BLOCK_FOLDER}/{width}x{height}"
            .Substring("Assets".Length);
        string[] files = { };
        if (Directory.Exists(absolutePath))
        {
            files = Directory.GetFiles(absolutePath, "*.asset", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                string unityPath = file.Substring(file.IndexOf("Assets/")).Replace(@"\", "/");
                UnityEngine.Object temp = AssetDatabase.LoadAssetAtPath(unityPath, typeof(ScriptableObject));
                blocks.Add(temp as ObstacleBlock);
            }
        }
        else
        {
            string rotatedPath = Application.dataPath +
            $"{Constants.OBSTACLE_BLOCK_FOLDER}/{height}x{width}"
            .Substring("Assets".Length);
            if (Directory.Exists(rotatedPath))
            {
                RotatedVersion = true;

                files = Directory.GetFiles(rotatedPath, "*.asset", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    string unityPath = file.Substring(file.IndexOf("Assets/")).Replace(@"\", "/");
                    UnityEngine.Object temp = AssetDatabase.LoadAssetAtPath(unityPath, typeof(ScriptableObject));
                    blocks.Add(temp as ObstacleBlock);
                }
            }
        }

        possibleBlocks = blocks.ToList();
    }

    public override bool StartUp(Vector3Int position, ITilemap readOnlyTilemap, GameObject go)
    {

        if (!Application.isPlaying)
        {
            Tilemap tilemap = readOnlyTilemap.GetComponent<Tilemap>();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    tilemap.SetTile(position + new Vector3Int(i, j, 0), null);
                }
            }

            LoadBlocks(position, tilemap);

            return false;
        }

        if (testBlock != null)
        {
            SubstituteWithObstacleBlock(position, testBlock);
        }
        else
        {
            ObstacleBlock block = possibleBlocks.PickRandom();

            SubstituteWithObstacleBlock(position, block);
        }

        return base.StartUp(position, readOnlyTilemap, go);
    }

    public void SubstituteWithObstacleBlock(Vector3Int position, ObstacleBlock block)
    {
        if (!(block.width == width && block.height == height) && !(RotatedVersion && block.width == height && block.height == width))
        {
            throw new Exception($"picked block width and block height does not correspond to tile's width and height");
        }

        Dictionary<Vector3Int, Tile> tilesDict = block.GetDictionary();
        if (!RotatedVersion)
        {
            for (int x = position.x; x < position.x + width; x++)
            {
                for (int y = position.y; y < position.y + height; y++)
                {
                    Vector3Int realPosition = new Vector3Int(x, y, position.z);
                    Vector3Int localPosition = new Vector3Int(x - position.x, y - position.y, position.z);
                    Tile tile = tilesDict[localPosition];
                    SetTileDelayManager.Instance.Enqueue(realPosition, tile);
                    //tilemap.SetTile(realPosition, tile);
                }
            }
        }
        else
        {
            for (int x = position.x; x < position.x + width; x++)
            {
                for (int y = position.y; y < position.y + height; y++)
                {
                    Vector3Int realPosition = new Vector3Int(x, y, position.z);
                    int reverse = ReverseNumber(y - position.y, 0, block.width - 1);
                    Vector3Int localPosition = new Vector3Int(reverse, x - position.x, position.z);
                    Tile tile = tilesDict[localPosition];
                    SetTileDelayManager.Instance.Enqueue(realPosition, tile);
                    //tilemap.SetTile(realPosition, tile);
                }
            }
        }
    }

    private int ReverseNumber(int num, int min, int max)
    {
        return (max + min) - num;
    }
}
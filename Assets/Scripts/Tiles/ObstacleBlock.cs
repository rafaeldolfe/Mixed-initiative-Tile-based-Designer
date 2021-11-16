using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class ObstacleBlock : ScriptableObject
{
    public int width;
    public int height;

    public List<Tile> tiles;

    public Dictionary<Vector3Int, Tile> GetDictionary()
    {
        Dictionary<Vector3Int, Tile> dict = new Dictionary<Vector3Int, Tile>();
        int index = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                dict[new Vector3Int(i, j, 0)] = tiles[index];
                index++;
            }
        }
        return dict;
    }
}
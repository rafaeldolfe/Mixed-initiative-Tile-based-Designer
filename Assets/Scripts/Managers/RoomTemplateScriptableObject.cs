using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomTemplateScriptableObject : ScriptableObject
{
    public RoomType type;
    public int width;
    public int height;
    public List<Direction> connections = new List<Direction>();
    public Vector3IntToTileDictionary tiles;

    public RoomTemplate GetPlainClass()
    {
        RoomTemplate plainRoom = new RoomTemplate
        {
            type = type,
            width = width,
            height = height,
            connections = new List<Direction>(connections),
            tilesDict = new Vector3IntToTileDictionary(tiles),
        };
        return plainRoom;
    }
}

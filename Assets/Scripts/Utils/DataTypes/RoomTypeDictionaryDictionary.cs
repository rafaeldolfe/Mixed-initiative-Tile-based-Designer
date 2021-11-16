using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class RoomTypeDictionaryDictionary : UnitySerializedDictionary<RoomType, RoomShapeRoomsDictionary>
{
    public RoomTypeDictionaryDictionary Clone()
    {
        return new RoomTypeDictionaryDictionary(this);
    }

    public RoomTypeDictionaryDictionary()
    {
    }

    public RoomTypeDictionaryDictionary(RoomTypeDictionaryDictionary toClone) : base(toClone)
    {
    }
}

[Serializable]
public class RoomShapeRoomsDictionary : UnitySerializedDictionary<RoomShape, List<RoomTemplateSetting>>
{
    public RoomShapeRoomsDictionary Clone()
    {
        return new RoomShapeRoomsDictionary(this);
    }

    public RoomShapeRoomsDictionary()
    {
    }

    public RoomShapeRoomsDictionary(RoomShapeRoomsDictionary toClone) : base(toClone)
    {
    }
}

[Serializable]
public class Vector3IntToTileDictionary : UnitySerializedDictionary<Vector3Int, Tile>
{
    public Vector3IntToTileDictionary Clone()
    {
        return new Vector3IntToTileDictionary(this);
    }

    public Vector3IntToTileDictionary()
    {
    }

    public Vector3IntToTileDictionary(Vector3IntToTileDictionary toClone) : base(toClone)
    {
    }

    public override string ToString()
    {
        return $"Count: {Count}";
    }
}

using ShapeGrammar;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    //public class Room
    public ShapeGenerator sg;

    private Level level;

    public RoomTypeDictionaryDictionary roomTypeToDictionaryDict;
    public Tilemap tilemap;
    public Tile wall;
    public SavedLevelScriptableObject levelToLoad;
    public MapManager mm;
    public Level GetLevel()
    {
        return level;
    }

    public void RecreateTilemap()
    {
        Transform tilemapParent = tilemap.transform.parent;
        DestroyImmediate(tilemap.gameObject);
        GameObject go = new GameObject("Level");
        tilemap = go.AddComponent<Tilemap>();
        go.AddComponent<TilemapRenderer>();
        go.transform.SetParent(tilemapParent);
    }

    [Button]
    public Level GenerateLevel(bool visualize = true, bool generateExtraWalls = true)
    {
        RecreateTilemap();

        Profiler.BeginSample("GenerateLevel");
        Reset();
        LoadRooms();

        SpaceState state = sg.GenerateShape(visualize);

        level = new Level { spaceState = state };

        if (generateExtraWalls)
        {
            Profiler.BeginSample("PopulateWithWalls(state.space);");
            PopulateWithWalls(state.space);
            Profiler.EndSample();
        }

        foreach (Cell cell in state.space)
        {
            Profiler.BeginSample("PlaceRandomRoomTemplate(cell);");
            PlaceRandomRoomTemplate(cell);
            Profiler.EndSample();
        }

        Profiler.EndSample();

        return level;
    }
    [Button]
    public void Generate100Levels(int num = 10)
    {
        for (int i = 0; i < num; i++)
        {
            GenerateLevel(false);
        }
    }
    private void PopulateWithWalls(List<Cell> space)
    {
        foreach (Cell cell in space)
        {
            int firstRoomWidth = 0;
            int firstRoomHeight = 0;
            if (roomTypeToDictionaryDict.ContainsKey(cell.roomType))
            {
                RoomShapeRoomsDictionary roomShapeRoomsDict = roomTypeToDictionaryDict[cell.roomType];
                if (roomShapeRoomsDict.ContainsKey(cell.connections.GetRoomShape()))
                {
                    firstRoomWidth = roomShapeRoomsDict[cell.connections.GetRoomShape()][0].room.width;
                    firstRoomHeight = roomShapeRoomsDict[cell.connections.GetRoomShape()][0].room.height;
                }
            }
            int offsetX = (cell.x - Constants.GRAPH_GRID_WIDTH / 2) * Constants.ROOM_TEMPLATE_WIDTH;
            int offsetY = (cell.y - Constants.GRAPH_GRID_HEIGHT / 2) * Constants.ROOM_TEMPLATE_HEIGHT;
            for (int j = -firstRoomHeight; j < 2 * firstRoomHeight; j++)
            {
                for (int i = -firstRoomWidth; i < 2 * firstRoomWidth; i++)
                {
                    Vector3Int tilemapPosition = new Vector3Int(offsetX + i, offsetY + j, 0);
                    if (!tilemap.HasTile(tilemapPosition))
                    {
                        tilemap.SetTile(tilemapPosition, wall);
                        level.tilePositions.Add(tilemapPosition);
                        level.tilePosToTile[tilemapPosition] = wall;
                        //level.tiles[tilemapPosition] = wall;
                    }
                }
            }
        }
    }
    private string GetFolder()
    {
        return $"Assets/Resources/{Constants.ROOM_TEMPLATE_RESOURCE_FOLDER}";
    }
    private void LoadRooms()
    {
        string resourcePath = GetFolder().ToResourcePath();
        RoomTemplateScriptableObject[] roomTemplates = Resources.LoadAll<RoomTemplateScriptableObject>(resourcePath);

        bool reload = false;

        foreach (RoomTemplateScriptableObject room in roomTemplates)
        {
            if (roomTypeToDictionaryDict.ContainsKey(room.type))
            {
                RoomShapeRoomsDictionary roomShapeRoomsDict = roomTypeToDictionaryDict[room.type];
                if (roomTypeToDictionaryDict[room.type].ContainsKey(room.connections.GetRoomShape()))
                {
                    if (roomShapeRoomsDict[room.connections.GetRoomShape()].Select(setting => setting.room).Contains(room))
                    {
                        continue;
                    }
                }
            }
            reload = true;
        }

        if (!reload)
        {
            return;
        }

        foreach (RoomTemplateScriptableObject room in roomTemplates)
        {
            RoomType roomType = room.type;
            RoomShapeRoomsDictionary shapeToRoomsDict;
            if (!roomTypeToDictionaryDict.ContainsKey(roomType) || roomTypeToDictionaryDict[roomType] == null)
            {
                roomTypeToDictionaryDict[roomType] = new RoomShapeRoomsDictionary();
            }

            shapeToRoomsDict = roomTypeToDictionaryDict[roomType];
            RoomShape shape = room.connections.GetRoomShape();
            if (!shapeToRoomsDict.ContainsKey(shape) || shapeToRoomsDict[shape] == null)
            {
                shapeToRoomsDict[shape] = new List<RoomTemplateSetting>();
            }

            shapeToRoomsDict[shape].Add(new RoomTemplateSetting { room = room, activated = true });
        }
    }

    private void Reset()
    {
        tilemap.ClearAllTiles();
    }

    private void OnValidate()
    {
        LoadRooms();
    }

    private Room PlaceRandomRoomTemplate(Cell cell)
    {
        Profiler.BeginSample("roomTypeToDictionaryDict[cell.roomType];");
        RoomShapeRoomsDictionary roomsDictionary = roomTypeToDictionaryDict[cell.roomType];
        if (!roomsDictionary.ContainsKey(cell.connections.GetRoomShape()))
        {
            Debug.LogWarning($"roomsDictionary did not contain shape {cell.connections.GetRoomShape()} for type {cell.roomType}");
            //throw new Exception($"roomsDictionary did not contain shape {cell.connections.GetRoomShape()} for type {cell.roomType}");
            return null;
        }
        Profiler.EndSample();
        Profiler.BeginSample("where -> select -> tolist");
        List<RoomTemplateSetting> settings = roomsDictionary[cell.connections.GetRoomShape()];
        List<RoomTemplate> rooms = settings
            .Where(setting => setting.activated)
            .Select(setting => setting.room.GetPlainClass())
            .ToList();

        Profiler.EndSample();
        Profiler.BeginSample("if (rooms.Count == 0)");
        if (rooms.Count == 0)
        {
            throw new Exception($"No rooms exist for the given cell's room type {cell.roomType} and cell's shape {cell.connections.GetRoomShape()}");
        }
        Profiler.EndSample();
        Profiler.BeginSample("rooms.PickRandom();");
        RoomTemplate room = rooms.PickRandom();
        Profiler.EndSample();

        return PlaceRoomTemplate(cell, room);
    }

    private Room PlaceRoomTemplate(Cell cell, RoomTemplate roomTemplate)
    {
        Profiler.BeginSample("PlaceRoomTemplate");
        AlignRoomToCell(roomTemplate, cell);
        int offsetX = (cell.x - Constants.GRAPH_GRID_WIDTH / 2) * Constants.ROOM_TEMPLATE_WIDTH;
        int offsetY = (cell.y - Constants.GRAPH_GRID_HEIGHT / 2) * Constants.ROOM_TEMPLATE_HEIGHT;

        Room room = new Room { template = roomTemplate };

        Dictionary<Vector3Int, Tile> tilesDict = roomTemplate.tilesDict;
        for (int j = 0; j < roomTemplate.height; j++)
        {
            for (int i = 0; i < roomTemplate.width; i++)
            {
                Vector3Int tilemapPosition = new Vector3Int(offsetX + i, offsetY + j, 0);
                Vector3Int localPosition = new Vector3Int(i, j, 0);
                bool success = tilesDict.TryGetValue(localPosition, out Tile tile);
                if (success)
                {
                    if (tile == null)
                    {
                        continue;
                    }
                    tilemap.SetTile(tilemapPosition, tile);

                    level.tilePositions.Add(tilemapPosition);
                    room.finalTilePositions.Add(tilemapPosition);

                    level.tilePosToTile[tilemapPosition] = tile;

                    //Tile finalTile = tilemap.GetTile(tilemapPosition) as Tile;

                    //room.finalTiles[tilemapPosition] = finalTile;
                    //level.tiles[tilemapPosition] = finalTile;

                    ScriptableTile sTile = tile as ScriptableTile;
                    OverflowTile oTile = tile as OverflowTile;
                    if (sTile != null)
                    {
                        if (sTile.tileType == SimpleTileType.Entrance)
                        {
                            level.entrance = tilemapPosition;
                        }
                        if (sTile.tileType == SimpleTileType.Goal)
                        {
                            level.goal = tilemapPosition;
                        }
                        foreach (SpecialFunctionality special in cell.specialFunctionalities)
                        {
                            if (sTile.specialNodeType == special.type)
                            {
                                switch (sTile.specialNodeType)
                                {
                                    case SpecialNodeType.Key:
                                        AddVector3IntToGuid(level.guidToKeys, tilemapPosition, special.guid);
                                        break;
                                    case SpecialNodeType.Lock:
                                        AddVector3IntToGuid(level.guidToLock, tilemapPosition, special.guid);
                                        break;
                                    case SpecialNodeType.KeyMulti:
                                        AddVector3IntToGuid(level.guidToKeyMultis, tilemapPosition, special.guid);
                                        break;
                                    case SpecialNodeType.LockMulti:
                                        AddVector3IntToGuid(level.guidToLockMulti, tilemapPosition, special.guid);
                                        break;
                                    default:
                                        break;
                                }
                                level.tilePosToGuid[tilemapPosition] = special.guid;
                                level.tilePosToCell[tilemapPosition] = cell;
                            }
                        }
                    }
                    //else if (oTile != null)
                    //{
                    //    foreach (SpecialFunctionality special in cell.specialFunctionalities)
                    //    {
                    //        if (oTile.specialNodeType == special.type)
                    //        {
                    //            for (int oi = 0; oi < oTile.width; oi++)
                    //            {
                    //                for (int oj = 0; oj < oTile.height; oj++)
                    //                {
                    //                    Vector3Int finalPos = tilemapPosition + new Vector3Int(oi, oj, 0);
                    //                    //if (room.positionToSpecialFunctionality.TryGetValue(finalPos, out List<SpecialFunctionality> list))
                    //                    //{
                    //                    //    room.positionToSpecialFunctionality[finalPos] = new List<SpecialFunctionality>();
                    //                    //}
                    //                    //room.positionToSpecialFunctionality[finalPos].Add(special);
                    //                    if (oTile.tileType == SimpleTileType.LockMulti)
                    //                    {
                    //                        if (level.guidToLockMulti.TryGetValue(special.guid, out List<Vector3Int> keys))
                    //                        {
                    //                            level.guidToLockMulti[special.guid] = new List<Vector3Int>();
                    //                        }
                    //                        level.guidToLockMulti[special.guid].Add(finalPos);
                    //                    }
                    //                    mm.SetFunctionalities(finalPos, cell.specialFunctionalities.Select(func => func.DeepCopy()).ToList());
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
        }
        level.rooms.Add(room);
        Profiler.EndSample();
        return room;
    }
    private void AddVector3IntToGuid(Dictionary<string, List<Vector3Int>> keyValuePairs, Vector3Int position, string guid)
    {
        if (!keyValuePairs.ContainsKey(guid))
        {
            keyValuePairs[guid] = new List<Vector3Int>();
        }
        keyValuePairs[guid].Add(position);
    }
    private void AlignRoomToCell(RoomTemplate room, Cell cell)
    {
        Direction transformDirection = GetTransformDirection(room, cell);

        RotateRoom(room, transformDirection);
    }

    private void RotateRoom(RoomTemplate room, Direction transformDirection)
    {
        int rotateNr = (int)transformDirection;

        Vector3IntToTileDictionary rotatedTiles = new Vector3IntToTileDictionary();
        foreach (KeyValuePair<Vector3Int, Tile> pair in room.tilesDict)
        {
            float halfWidth = ((float)room.width - 1) / 2;
            float halfHeight = ((float)room.height - 1) / 2;
            Vector3 rotatedPos = RotatePointAroundPivot(
                new Vector3(pair.Key.x, pair.Key.y),
                new Vector3(halfWidth, halfHeight),
                new Vector3(0, 0, 90 * rotateNr));

            int x = Mathf.RoundToInt(rotatedPos.x);
            int y = Mathf.RoundToInt(rotatedPos.y);

            int obsX = 0;
            int obsY = 0;

            if (pair.Value != null
            && (pair.Value.GetType().IsSubclassOf(typeof(OverflowTile))
            || pair.Value.GetType() == typeof(OverflowTile)))
            {
                OverflowTile overflowTile = (OverflowTile)pair.Value;
                bool invertBlock = false;
                switch (rotateNr)
                {
                    case 1:
                        obsX = -(overflowTile.height - 1);
                        invertBlock = true;
                        break;

                    case 2:
                        obsY = -(overflowTile.height - 1);
                        obsX = -(overflowTile.width - 1);
                        break;

                    case 3:
                        obsY = -(overflowTile.width - 1);
                        invertBlock = true;
                        break;

                    default:
                        break;
                }
                if (invertBlock && overflowTile.invertedVersion != null)
                {
                    rotatedTiles[new Vector3Int(x + obsX, y + obsY, 0)] = overflowTile.invertedVersion;
                }
                else
                {
                    rotatedTiles[new Vector3Int(x + obsX, y + obsY, 0)] = overflowTile;
                }
            }
            else
            {
                if (rotatedTiles.ContainsKey(new Vector3Int(x, y, 0))
                    && rotatedTiles[new Vector3Int(x, y, 0)] != null)
                {
                    //if (rotatedTiles[new Vector3Int(x, y, 0)].GetType().IsSubclassOf(typeof(OverflowTile))
                    //    || rotatedTiles[new Vector3Int(x, y, 0)].GetType() == typeof(OverflowTile))
                    //{
                    //    Debug.Log($"Tried to override {typeof(OverflowTile)}");
                    //}
                }
                else
                {
                    rotatedTiles[new Vector3Int(x, y, 0)] = pair.Value;
                }
            }
        }

        room.tilesDict = rotatedTiles;
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    private Direction GetTransformDirection(RoomTemplate room, Cell cell)
    {
        List<Direction> directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();
        for (int i = 0; i < directions.Count; i++)
        {
            Direction current = directions[i];

            List<Direction> transformedDirections = room.connections.Select(dir => dir.TransformDirection(current)).ToList();

            if (cell.connections.TrueForAll(dir => transformedDirections.Contains(dir)))
            {
                return current;
            }
        }
        throw new Exception($"There was no way to align connection between room {room} and cell {cell}");
    }
}

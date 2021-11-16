using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class RoomTemplatesManager : MonoBehaviour
{
    public RoomType roomType;
    public int sceneWidth;
    public int sceneHeight;
    public int roomTemplateWidth;
    public int roomTemplateHeight;
    public Tilemap tilemap;
    public Tile border;
    public Tile opening;
    public Tile emptyTile;
    public Tile wallTile;
    public int spacing;
    private const int BORDER_SIZE = 1;

    [Button]
    public void GenerateBorders()
    {
        for (int i = 0; i < sceneWidth; i++)
        {
            int offsetX = i * (BORDER_SIZE * 2 + roomTemplateWidth + spacing);
            for (int j = 0; j < sceneHeight; j++)
            {
                int offsetY = j * (BORDER_SIZE * 2 + roomTemplateHeight + spacing);

                bool isRoomTemplateNew = true;

                // Check if everything inside room is null
                for (int x = 0; x < roomTemplateWidth; x++)
                {
                    for (int y = 0; y < roomTemplateHeight; y++)
                    {
                        Vector3Int tilePos = new Vector3Int(x + offsetX, y + offsetY, 0);
                        if (tilemap.HasTile(tilePos))
                        {
                            isRoomTemplateNew = false;
                        }
                    }
                }

                for (int x = -1; x < roomTemplateWidth + 1; x++)
                {
                    for (int y = -1; y < roomTemplateHeight + 1; y++)
                    {
                        if (x == -1 || y == -1 || x == roomTemplateWidth || y == roomTemplateHeight)
                        {
                            if (!tilemap.GetTile(new Vector3Int(x + offsetX, y + offsetY, 0)) == opening)
                            {
                                tilemap.SetTile(new Vector3Int(x + offsetX, y + offsetY, 0), border);
                            }
                        }
                        else if (isRoomTemplateNew && !tilemap.HasTile(new Vector3Int(x + offsetX, y + offsetY, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x + offsetX, y + offsetY, 0), wallTile);
                        }
                    }
                }
            }
        }
        IdentifyExits();
    }
    [Button]
    public void SaveBlocks()
    {
        if (!SceneManager.GetActiveScene().name.ToLower().Contains(roomType.ToString().ToLower()))
        {
            throw new System.Exception($"Name of scene does not contain room type in its name");
        }
        IdentifyExits();
        DeleteRoomShapeFolders();
        for (int i = 0; i < sceneWidth; i++)
        {
            int offsetX = i * (BORDER_SIZE * 2 + roomTemplateWidth + spacing);
            for (int j = 0; j < sceneHeight; j++)
            {
                int offsetY = j * (BORDER_SIZE * 2 + roomTemplateHeight + spacing);

                Vector3IntToTileDictionary block = new Vector3IntToTileDictionary();

                for (int x = 0; x < roomTemplateWidth; x++)
                {
                    for (int y = 0; y < roomTemplateHeight; y++)
                    {
                        Tile tile = tilemap.GetTile(new Vector3Int(x + offsetX, y + offsetY, 0)) as Tile;
                        block[new Vector3Int(x, y, 0)] = tile;
                    }
                }

                RoomTemplateScriptableObject asset = ScriptableObject.CreateInstance<RoomTemplateScriptableObject>();

                asset.width = roomTemplateWidth;
                asset.height = roomTemplateHeight;
                asset.tiles = block;
                asset.type = roomType;

                #region calculate connections for RoomTemplate

                List<Vector3Int> tilesToCheck = new List<Vector3Int>();
                //
                //
                // Left side of the room template
                //
                //
                if (roomTemplateHeight % 2 == 0)
                {
                    tilesToCheck.Add(new Vector3Int(offsetX - 1, offsetY + roomTemplateHeight / 2, 0));
                    tilesToCheck.Add(new Vector3Int(offsetX - 1, offsetY + roomTemplateHeight / 2 - 1, 0));
                }
                else
                {
                    tilesToCheck.Add(new Vector3Int(offsetX - 1, offsetY + Mathf.FloorToInt(roomTemplateHeight / 2.0f), 0));
                }
                if (tilesToCheck.TrueForAll(v => tilemap.GetTile(v) == opening))
                {
                    asset.connections.Add(Direction.LEFT);
                }
                //
                //
                // Right side of the room template
                //
                //

                tilesToCheck.Clear();
                if (roomTemplateHeight % 2 == 0)
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + 1 + roomTemplateWidth - 1, offsetY + roomTemplateHeight / 2, 0));
                    tilesToCheck.Add(new Vector3Int(offsetX + 1 + roomTemplateWidth - 1, offsetY + roomTemplateHeight / 2 - 1, 0));
                }
                else
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + 1 + roomTemplateWidth - 1, offsetY + Mathf.FloorToInt(roomTemplateHeight / 2.0f), 0));
                }
                if (tilesToCheck.TrueForAll(v => tilemap.GetTile(v) == opening))
                {
                    asset.connections.Add(Direction.RIGHT);
                }

                //
                //
                // Bottom side of the room template
                //
                //

                tilesToCheck.Clear();
                if (roomTemplateWidth % 2 == 0)
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth / 2, offsetY - 1, 0));
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth / 2 - 1, offsetY - 1, 0));
                }
                else
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + Mathf.FloorToInt(roomTemplateWidth / 2.0f), offsetY - 1, 0));
                }
                if (tilesToCheck.TrueForAll(v => tilemap.GetTile(v) == opening))
                {
                    asset.connections.Add(Direction.DOWN);
                }

                //
                //
                // Top side of the room template
                //
                //

                tilesToCheck.Clear();
                if (roomTemplateWidth % 2 == 0)
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth / 2, offsetY + 1 + roomTemplateHeight - 1, 0));
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth / 2 - 1, offsetY + 1 + roomTemplateHeight - 1, 0));
                }
                else
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + Mathf.FloorToInt(roomTemplateWidth / 2.0f), offsetY + 1 + roomTemplateHeight - 1, 0));
                }
                if (tilesToCheck.TrueForAll(v => tilemap.GetTile(v) == opening))
                {
                    asset.connections.Add(Direction.UP);
                }

                #endregion calculate connections for RoomTemplate

                RoomShape roomShape = asset.connections.GetRoomShape();
                string finalFolder = $"{GetFolder()}/{roomShape}";

                if (roomShape == RoomShape.None)
                {
                    continue;
                }

                CreateAssetInFolder(asset, $"{finalFolder}", $"Room{j + i * sceneWidth}");
                AssetDatabase.SaveAssets();
            }
        }
    }


    private string GetFolder()
    {
        return $"Assets/Resources/{Constants.ROOM_TEMPLATE_RESOURCE_FOLDER}/{roomType}";
    }

    private void IdentifyExits()
    {
        for (int i = 0; i < sceneWidth; i++)
        {
            int offsetX = i * (BORDER_SIZE * 2 + roomTemplateWidth + spacing);
            for (int j = 0; j < sceneHeight; j++)
            {
                int offsetY = j * (BORDER_SIZE * 2 + roomTemplateHeight + spacing);

                List<Vector3Int> tilesToCheck = new List<Vector3Int>();
                //
                //
                // Left side of the room template
                //
                //
                if (roomTemplateHeight % 2 == 0)
                {
                    tilesToCheck.Add(new Vector3Int(offsetX, offsetY + roomTemplateHeight / 2, 0));
                    tilesToCheck.Add(new Vector3Int(offsetX, offsetY + roomTemplateHeight / 2 - 1, 0));
                }
                else
                {
                    tilesToCheck.Add(new Vector3Int(offsetX, offsetY + Mathf.FloorToInt(roomTemplateHeight / 2.0f), 0));
                }
                if (CheckAllTilesAreEmptyOrNull(tilesToCheck))
                {
                    foreach (Vector3Int v in tilesToCheck)
                    {
                        Vector3Int position = new Vector3Int(v.x - 1, v.y, v.z);
                        tilemap.SetTile(position, opening);
                        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 180f), Vector3.one);
                        tilemap.SetTransformMatrix(position, matrix);
                    }
                }
                else
                {
                    foreach (Vector3Int v in tilesToCheck)
                    {
                        Vector3Int position = new Vector3Int(v.x - 1, v.y, v.z);
                        tilemap.SetTile(position, border);
                        tilemap.SetTransformMatrix(position, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one));
                    }
                }

                //
                //
                // Right side of the room template
                //
                //

                tilesToCheck.Clear();
                if (roomTemplateHeight % 2 == 0)
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth - 1, offsetY + roomTemplateHeight / 2, 0));
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth - 1, offsetY + roomTemplateHeight / 2 - 1, 0));
                }
                else
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth - 1, offsetY + Mathf.FloorToInt(roomTemplateHeight / 2.0f), 0));
                }
                if (CheckAllTilesAreEmptyOrNull(tilesToCheck))
                {
                    foreach (Vector3Int v in tilesToCheck)
                    {
                        Vector3Int position = new Vector3Int(v.x + 1, v.y, v.z);
                        tilemap.SetTile(position, opening);
                    }
                }
                else
                {
                    foreach (Vector3Int v in tilesToCheck)
                    {
                        Vector3Int position = new Vector3Int(v.x + 1, v.y, v.z);
                        tilemap.SetTile(position, border);
                        tilemap.SetTransformMatrix(position, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one));
                    }
                }

                //
                //
                // Bottom side of the room template
                //
                //

                tilesToCheck.Clear();
                if (roomTemplateWidth % 2 == 0)
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth / 2, offsetY, 0));
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth / 2 - 1, offsetY, 0));
                }
                else
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + Mathf.FloorToInt(roomTemplateWidth / 2.0f), offsetY, 0));
                }
                if (CheckAllTilesAreEmptyOrNull(tilesToCheck))
                {
                    foreach (Vector3Int v in tilesToCheck)
                    {
                        Vector3Int position = new Vector3Int(v.x, v.y - 1, v.z);
                        tilemap.SetTile(position, opening);
                        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 270f), Vector3.one);
                        tilemap.SetTransformMatrix(position, matrix);
                    }
                }
                else
                {
                    foreach (Vector3Int v in tilesToCheck)
                    {
                        Vector3Int position = new Vector3Int(v.x, v.y - 1, v.z);
                        tilemap.SetTile(position, border);
                        tilemap.SetTransformMatrix(position, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one));
                    }
                }

                //
                //
                // Top side of the room template
                //
                //

                tilesToCheck.Clear();
                if (roomTemplateWidth % 2 == 0)
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth / 2, offsetY + roomTemplateHeight - 1, 0));
                    tilesToCheck.Add(new Vector3Int(offsetX + roomTemplateWidth / 2 - 1, offsetY + roomTemplateHeight - 1, 0));
                }
                else
                {
                    tilesToCheck.Add(new Vector3Int(offsetX + Mathf.FloorToInt(roomTemplateWidth / 2.0f), offsetY + roomTemplateHeight - 1, 0));
                }
                if (CheckAllTilesAreEmptyOrNull(tilesToCheck))
                {
                    foreach (Vector3Int v in tilesToCheck)
                    {
                        Vector3Int position = new Vector3Int(v.x, v.y + 1, v.z);
                        tilemap.SetTile(position, opening);
                        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 90f), Vector3.one);
                        tilemap.SetTransformMatrix(position, matrix);
                    }
                }
                else
                {
                    foreach (Vector3Int v in tilesToCheck)
                    {
                        Vector3Int position = new Vector3Int(v.x, v.y + 1, v.z);
                        tilemap.SetTile(position, border);
                        tilemap.SetTransformMatrix(position, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one));
                    }
                }
            }
        }
    }

    private bool CheckAllTilesAreEmptyOrNull(List<Vector3Int> tiles)
    {
        return tiles.TrueForAll(v =>
        {
            Tile tile = tilemap.GetTile(v) as Tile;
            return tile == emptyTile || tile == null;
        });
    }

    private void DeleteRoomShapeFolders()
    {
        string[] roomShapes = AssetDatabase.GetSubFolders($"Assets/Resources/{Constants.ROOM_TEMPLATE_RESOURCE_FOLDER}/{roomType}");

        foreach (string directory in roomShapes)
        {
            FileUtil.DeleteFileOrDirectory(directory);
        }
    }

    private void DeleteUnusedShapes(List<string> usedShapes)
    {
        string[] roomShapes = AssetDatabase.GetSubFolders($"Assets/Resources/{Constants.ROOM_TEMPLATE_RESOURCE_FOLDER}/{roomType}");

        List<string> unusedDirectories = roomShapes.Where(shape => !usedShapes.Any(e => shape.Contains(e))).ToList();

        foreach (string directory in unusedDirectories)
        {
            FileUtil.DeleteFileOrDirectory(directory);
        }
    }

    private void MoveAssetIntoFolderDangerously(string oldAssetPath, string newAssetPath)
    {
        string[] pathSegments = newAssetPath.Split(new char[] { '/' });
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

        if (File.Exists(newAssetPath))
        {
            AssetDatabase.DeleteAsset(newAssetPath);
        }

        AssetDatabase.MoveAsset(oldAssetPath, newAssetPath);
    }

    private void CreateAssetInFolder(UnityEngine.Object newAsset, string Folder, string AssetName)
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

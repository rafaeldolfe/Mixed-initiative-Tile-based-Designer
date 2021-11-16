using Priority_Queue;
using ShapeGrammar;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThreadedPathfinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;

namespace ExpressiveRange
{
    public class ExpressiveRangeAnalyzer : MonoBehaviour
    {
        public MapManager mm;
        public LevelGenerator lg;
        public string runName;
        public string displayName;
        public int number_of_samples;
        private readonly List<Sample> samples = new List<Sample>();
        [Button]
        public void GenerateAnalysis(bool save = true)
        {
            StartCoroutine(StartLevelGenerationCoroutine(number_of_samples, save));
        }
        private IEnumerator StartLevelGenerationCoroutine(int number_of_samples, bool save)
        {
            SetTileDelayManager manager = SetTileDelayManager.Instance;
            samples.Clear();
            for (int i = 0; i < number_of_samples; i++)
            {
                Profiler.BeginSample("lg.GenerateLevel");
                Level level = lg.GenerateLevel(number_of_samples == 1, false);
                Profiler.EndSample();

                yield return null;

                while (!manager.Finished())
                {
                    yield return null;
                }

                yield return null;

                Profiler.BeginSample("calculate leniency and linearity");
                double leniency = CalculateLeniency(level);
                double linearity = CalculateLinearityPerTile(level);
                Profiler.EndSample();

                samples.Add(new Sample { level = level, leniency = leniency, linearity = linearity });

                Debug.Log($"{i}");
            };

            Profiler.BeginSample("SaveData(save);");
            SaveData(save);
            Profiler.EndSample();
        }

        private void SaveData(bool save)
        {
            string unityFolderPath = $"Assets/ExpressiveRangeAnalyses";
            string systemFolderPath = $"{Path.GetDirectoryName(Application.dataPath)}\\Assets\\ExpressiveRangeAnalyses";

            if (save)
            {
                if (!AssetDatabase.IsValidFolder($"{unityFolderPath}/{runName}-{number_of_samples}"))
                {
                    AssetDatabase.CreateFolder(unityFolderPath, $"{runName}-{number_of_samples}");
                }
                //else
                //{
                //    throw new Exception($"Will not override folder with new data. Must delete folder or choose a unique folder name.");
                //}
            }

            unityFolderPath += $"/{runName}-{number_of_samples}";

            StringBuilder sb = new StringBuilder();

            sb.Append($"{displayName}\n");

            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < samples.Count; i++)
                {
                    Sample sample = samples[i];
                    sb.Append($"{i}\n");
                    if (save)
                    {
                        Profiler.BeginSample("SaveData(save);");
                        SaveLevel(sample.level, unityFolderPath, i);
                        Profiler.EndSample();
                    }
                    sb.Append($"{sample.leniency}\n");
                    sb.Append($"{sample.linearity}\n");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();

            if (save)
            {
                File.WriteAllText($"{systemFolderPath}\\{runName}-{number_of_samples}\\data.txt", sb.ToString());
            }

            if (save)
            {
                EditorUtility.SetDirty(this);
            }

            AssetDatabase.Refresh();
        }

#if UNITY_EDITOR
        private void SaveLevel(Level level, string unityFolderPath, int id)
        {
            List<SavedTile> savedTiles = new List<SavedTile>();

            Dictionary<Tile, string> cachedTileToGuid = new Dictionary<Tile, string>();

            foreach (KeyValuePair<Vector3Int, Tile> pair in level.tilePosToTile)
            {
                Vector3Int pos = pair.Key;
                Tile tile = pair.Value;
                if (tile == null)
                {
                    continue;
                }
                if (!cachedTileToGuid.ContainsKey(tile))
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tile, out string guid, out long localId);
                    cachedTileToGuid[tile] = guid;
                }
                savedTiles.Add(new SavedTile { guid = cachedTileToGuid[tile], position = pos });
            }

            SavedLevelScriptableObject savedLevel = ScriptableObject.CreateInstance<SavedLevelScriptableObject>();
            savedLevel.savedTiles = savedTiles;
            savedLevel.name = $"{id}";
            AssetDatabase.CreateAsset(savedLevel, $"{unityFolderPath}/{id}.asset");
        }
        [Button]
        public void SaveLevel()
        {
            List<SavedTile> savedTiles = new List<SavedTile>();

            foreach (Vector3Int position in lg.tilemap.cellBounds.allPositionsWithin)
            {
                if (lg.tilemap.HasTile(position))
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(lg.tilemap.GetTile(position), out string guid, out long localId);
                    savedTiles.Add(new SavedTile { guid = guid, position = position });
                }
            }

            SavedLevelScriptableObject savedLevel = ScriptableObject.CreateInstance<SavedLevelScriptableObject>();
            savedLevel.savedTiles = savedTiles;
            savedLevel.name = $"SavedLevel with {savedTiles.Count} tiles";
            AssetDatabase.CreateAsset(savedLevel, $"{Constants.SAVED_LEVELS_PATH}/{savedLevel.name}.asset");
            AssetDatabase.SaveAssets();
        }
        public string fullRunName;
        [Button]
        public void LoadLevel(int id)
        {
            fullRunName = this.fullRunName;
            if (id < 0)
            {
                return;
            }

            string unityFolderPath = $"Assets/ExpressiveRangeAnalyses/{fullRunName}/{id}.asset";

            SavedLevelScriptableObject savedLevel = AssetDatabase.LoadAssetAtPath<SavedLevelScriptableObject>($"{unityFolderPath}");

            lg.RecreateTilemap();

            foreach (SavedTile savedTile in savedLevel.savedTiles)
            {
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(savedTile.guid));
                lg.tilemap.SetTile(savedTile.position, tile);
            }
        }
#endif
        private double CalculateLeniency(Level level)
        {
            int validTileCount = 0;
            double averageTileLeniency = 0.0f;
            foreach (Room room in level.rooms)
            {
                foreach (Vector3Int tile in room.finalTilePositions)
                {
                    ScriptableTile sTile = lg.tilemap.GetTile(tile) as ScriptableTile;
                    if (sTile != null)
                    {
                        if (sTile.tileType == SimpleTileType.Wall)
                        {
                            continue;
                        }
                        validTileCount++;
                        averageTileLeniency += GetLeniency(sTile.tileType);
                    }
                }
            }
            return averageTileLeniency / validTileCount;
        }
        private double CalculateLinearityPerTile(Level level)
        {
            Vector3Int start = level.entrance;
            Vector3Int goal = level.goal;
            Vector2 v = new Vector2(start.x, start.y);
            Vector2 w = new Vector2(goal.x, goal.y);

            float l2 = Mathf.Pow(w.x - v.x, 2) + Mathf.Pow(w.y - v.y, 2);

            List<Vector2Int> relevantTiles = level.tilePosToTile
                .Where(pair => !pair.Value.name.ToLower().Contains("wall"))
                .Select(pair => new Vector2Int(pair.Key.x, pair.Key.y))
                .ToList();

            float averageDistanceToLine = 0;
            foreach (var item in relevantTiles)
            {
                Vector2 p = new Vector2(item.x, item.y);

                if (l2 == 0.0)
                {
                    averageDistanceToLine -= Vector2.Distance(v, p) / relevantTiles.Count;
                }

                float t = Math.Max(0, Math.Min(1, Vector2.Dot(p - v, w - v) / l2));

                Vector2 projection = v + t * (w - v);

                averageDistanceToLine -= Vector2.Distance(p, projection) / relevantTiles.Count;
            }
            //foreach (Cell cell in level.spaceState.space)
            //{
            //    Vector2 p = new Vector2(cell.x, cell.y);

            //    if (l2 == 0.0)
            //    {
            //        averageDistanceToLine -= Vector2.Distance(v, p) / level.spaceState.space.Count;
            //        break;
            //    }

            //    float t = Math.Max(0, Math.Min(1, Vector2.Dot(p - v, w - v) / l2));

            //    Vector2 projection = v + t * (w - v);

            //    averageDistanceToLine -= Vector2.Distance(p, projection) / level.spaceState.space.Count;
            //}

            return averageDistanceToLine;
        }
        private double CalculateLinearity(Level level)
        {
            Vector3Int start = level.entrance;
            Vector3Int goal = level.goal;
            //4x + 2y - 4
            //4x - 4 = -2y
            //-2x + 2 = y
            //4
            //2
            //-(12 + 2 * -4) = -4
            Cell entranceCell = level.spaceState.space.Find(cell => cell.roomType == RoomType.Entrance);
            Cell goalCell = level.spaceState.space.Find(cell => cell.roomType == RoomType.Goal);
            Vector2 cellStart = new Vector2(entranceCell.x, entranceCell.y);
            Vector2 cellGoal = new Vector2(goalCell.x, goalCell.y);
            Vector2 v = new Vector2(cellStart.x, cellStart.y);
            Vector2 w = new Vector2(cellGoal.x, cellGoal.y);

            float l2 = Mathf.Pow(w.x - v.x, 2) + Mathf.Pow(w.y - v.y, 2);

            float averageDistanceToLine = 0;
            foreach (Cell cell in level.spaceState.space)
            {
                Vector2 p = new Vector2(cell.x, cell.y);

                if (l2 == 0.0)
                {
                    averageDistanceToLine -= Vector2.Distance(v, p) / level.spaceState.space.Count;
                    break;
                }

                float t = Math.Max(0, Math.Min(1, Vector2.Dot(p - v, w - v) / l2));

                Vector2 projection = v + t * (w - v);

                averageDistanceToLine -= Vector2.Distance(p, projection) / level.spaceState.space.Count;
            }

            return averageDistanceToLine;
        }
        //private double CalculateLinearity(Level level)
        //{
        //    Vector3Int start = level.entrance;
        //    Vector3Int goal = level.goal;
        //    //4x + 2y - 4
        //    //4x - 4 = -2y
        //    //-2x + 2 = y
        //    //4
        //    //2
        //    //-(12 + 2 * -4) = -4
        //    double a = start.y - goal.y;
        //    double b = goal.x - start.x;
        //    double c = -(a * (goal.x) + b * (goal.y));

        //    double averageDistanceToLine = 0;
        //    foreach (Cell cell in level.spaceState.space)
        //    {
        //        Vector2Int cellPoint = new Vector2Int(cell.x, cell.y);

        //        // 2 1

        //        //4 * 2 + 2 * 1 - 4 = 6

        //        //6 / sqrt((4 * 4 + 2 * 2))

        //        double distance = Math.Abs(a * cellPoint.x + b * cellPoint.y + c) / Math.Sqrt(a * a + b * b);

        //        averageDistanceToLine -= distance / level.spaceState.space.Count;
        //    }

        //    return averageDistanceToLine;
        //}
        public class Lock
        {
            public enum LockType
            {
                Lock,
                LockMulti,
            }
            public LockType lockType;
            public string guid;
        }
        private class StringListScriptableTiles
        {
            public string guid;
            public List<ScriptableTile> sTiles = new List<ScriptableTile>();
            public SpecialNodeType nodeType;
        }
        //private float CalculateShortestPathToWinningPath(Level level)
        //{
        //    Pathfinding pf = new Pathfinding();

        //    List<string> locksToUnlockBeforeGoal = new List<string>();

        //    Vector3Int start = level.entrance;
        //    Vector3Int end = level.goal;

        //    List<Node> nodesFromGoalToEntrance = pf.AStarRun(start.x, start.y, end.x, end.y, level);

        //    //List<ScriptableTile> locks = new List<ScriptableTile>();
        //    //Dictionary<string, List<ScriptableTile>> locks = new Dictionary<string, List<ScriptableTile>>();

        //    List<StringListScriptableTiles> locks = new List<StringListScriptableTiles>();

        //    //List<KeyValuePair<string, List<ScriptableTile>>> locks = new List<KeyValuePair<string, List<ScriptableTile>>>();

        //    for (int i = 0; i < nodesFromGoalToEntrance.Count; i++)
        //    {
        //        Node node = nodesFromGoalToEntrance[i];

        //        Tile tile = level.tiles[new Vector3Int(node.X, node.Y, 0)];
        //        Vector3Int tilePos = new Vector3Int(node.X, node.Y, 0);

        //        ScriptableTile sTile = tile as ScriptableTile;

        //        if (sTile.specialNodeType == SpecialNodeType.Lock || sTile.specialNodeType == SpecialNodeType.LockMulti)
        //        {
        //            string guid = level.tilePosToGuid[tilePos];
        //            var prev = locks.Find(stst => stst.guid == guid);
        //            if (prev == null)
        //            {
        //                StringListScriptableTiles guidAndTileList = new StringListScriptableTiles { guid = guid, nodeType = sTile.specialNodeType };
        //                guidAndTileList.sTiles.Add(sTile);
        //                locks.Add(guidAndTileList);
        //            }
        //            else
        //            {
        //                if (prev.nodeType != sTile.specialNodeType)
        //                {
        //                    throw new Exception($"Something went awfully wrong. Expected nodetype: {sTile.specialNodeType}, found {prev.nodeType}");
        //                }
        //                prev.sTiles.Add(sTile);
        //            }
        //            //if (!locks.ContainsKey(guid))
        //            //{
        //            //    locks[guid] = new List<ScriptableTile>();
        //            //}
        //            //locks[guid].Add(sTile);
        //        }
        //    }

        //    for (int i = 0; i < locks.Count; i++)
        //    {
        //        var stst = locks[i];
        //        List<Vector3Int> keyPositions = new List<Vector3Int>();
        //        if (stst.nodeType == SpecialNodeType.Lock)
        //        {
        //            keyPositions = GetAllKeys(stst.guid, level);
        //        }
        //        else
        //        {
        //            keyPositions = GetAllKeyMultis(stst.guid, level);
        //        }

        //        //CalculateAverageLength
        //    }

        //    return 0;
        //}
        private List<Vector3Int> GetAllKeys(string guid, Level level)
        {
            return level.guidToKeys[guid];
        }
        private List<Vector3Int> GetAllKeyMultis(string guid, Level level)
        {
            return level.guidToKeyMultis[guid];
        }
        private float GetLeniency(SimpleTileType tileType)
        {
            switch (tileType)
            {
                case SimpleTileType.Spike:
                    return -0.25f;
                case SimpleTileType.Gap:
                    return -0.5f;
                case SimpleTileType.Monster:
                    return -1.0f;
                case SimpleTileType.Treasure:
                    return 1.0f;
                default:
                    return 0.0f;
            }
        }
        //public class Pathfinding
        //{
        //    public const int MAX = 1000;
        //    public const float DIAGONAL_DST = 1.41421356237f;

        //    private FastPriorityQueue<Node> open = new FastPriorityQueue<Node>(MAX);
        //    private Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        //    private Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();
        //    private List<Node> near = new List<Node>();

        //    public Pathfinding()
        //    {

        //    }

        //    public List<Node> AStarRun(int startX, int startY, int endX, int endY, Level level)
        //    {
        //        // Clear everything up.
        //        Clear();

        //        var start = Node.Create(startX, startY);
        //        var end = Node.Create(endX, endY);

        //        // Check the start/end relationship 
        //        // IT IS NOW LEGAL TO TARGET WHERE START IS END.
        //        if (start.Equals(end))
        //        {
        //            return new List<Node> { start };
        //        }

        //        // Add the starting point to all relevant structures.
        //        open.Enqueue(start, 0f);
        //        cameFrom[start] = start;
        //        costSoFar[start] = 0f;

        //        int count;
        //        while ((count = open.Count) > 0)
        //        {
        //            // Detect if the current open amount exceeds the capacity.
        //            // This only happens in very large open areas. Corridors and hallways will never cause this, not matter how large the actual path length.
        //            if (count >= MAX - 8)
        //            {
        //                return null;
        //            }

        //            var current = open.Dequeue();

        //            if (current.Equals(end))
        //            {
        //                // We found the end of the path!
        //                List<Node> tracedPath = TracePath(end);
        //                return tracedPath;
        //            }

        //            // Get all neighbours (tiles that can be walked on to)
        //            var neighbours = GetNear(current, level);
        //            foreach (Node n in neighbours)
        //            {
        //                float newCost = costSoFar[current] + 1; // Note that this could change depending on speed changes per-tile. Currently not implemented.

        //                if (!costSoFar.ContainsKey(n) || newCost < costSoFar[n])
        //                {
        //                    costSoFar[n] = newCost;
        //                    float priority = newCost + Heuristic(n, end);
        //                    open.Enqueue(n, priority);
        //                    cameFrom[n] = current;
        //                }
        //            }
        //        }

        //        return null;
        //    }
        //    private void ResetFields()
        //    {
        //        cameFrom = new Dictionary<Node, Node>();
        //        costSoFar = new Dictionary<Node, float>();
        //    }
        //    private List<Node> TracePath(Node end)
        //    {
        //        List<Node> path = new List<Node>();
        //        Node child = end;

        //        bool run = true;
        //        while (run)
        //        {
        //            Node previous = cameFrom[child];
        //            path.Add(child);
        //            if (previous != null && child != previous)
        //            {
        //                child = previous;
        //            }
        //            else
        //            {
        //                run = false;
        //            }
        //        }

        //        path.Reverse();
        //        return path;
        //    }
        //    public void Clear()
        //    {
        //        costSoFar.Clear();
        //        cameFrom.Clear();
        //        near.Clear();
        //        open.Clear();
        //    }
        //    private float Abs(float x)
        //    {
        //        if (x < 0)
        //            return -x;
        //        else
        //            return x;
        //    }
        //    private float Heuristic(Node a, Node b)
        //    {
        //        // Gives a rough distance.
        //        return Abs(a.X - b.X) + Abs(a.Y - b.Y);
        //    }
        //    private List<Node> GetNear(Node node, Level level)
        //    {
        //        // Want to add nodes connected to the center node, if they are walkable.
        //        // This code stops the pathfinder from cutting corners, and going through walls that are diagonal from each other.

        //        near.Clear();
        //        // Left
        //        ScriptableTile leftSTile = level.tiles[new Vector3Int(node.X - 1, node.Y, 0)] as ScriptableTile;
        //        ChildTile leftChildTile = level.tiles[new Vector3Int(node.X - 1, node.Y, 0)] as ChildTile;
        //        if (leftChildTile != null)
        //        {
        //            leftSTile = leftChildTile.parent;
        //        }
        //        if (leftSTile != null && leftSTile.walkability == 1)
        //        {
        //            near.Add(Node.Create(node.X - 1, node.Y));
        //        }

        //        ScriptableTile rightSTile = level.tiles[new Vector3Int(node.X + 1, node.Y, 0)] as ScriptableTile;
        //        ChildTile rightChildTile = level.tiles[new Vector3Int(node.X + 1, node.Y, 0)] as ChildTile;
        //        if (rightChildTile != null)
        //        {
        //            rightSTile = rightChildTile.parent;
        //        }
        //        if (rightSTile != null && rightSTile.walkability == 1)
        //        {
        //            near.Add(Node.Create(node.X + 1, node.Y));
        //        }

        //        ScriptableTile upSTile = level.tiles[new Vector3Int(node.X, node.Y + 1, 0)] as ScriptableTile;
        //        ChildTile upChildTile = level.tiles[new Vector3Int(node.X, node.Y + 1, 0)] as ChildTile;
        //        if (upChildTile != null)
        //        {
        //            upSTile = upChildTile.parent;
        //        }
        //        if (upSTile != null && upSTile.walkability == 1)
        //        {
        //            near.Add(Node.Create(node.X, node.Y + 1));
        //        }

        //        ScriptableTile downSTile = level.tiles[new Vector3Int(node.X, node.Y - 1, 0)] as ScriptableTile;
        //        ChildTile downChildTile = level.tiles[new Vector3Int(node.X, node.Y - 1, 0)] as ChildTile;
        //        if (downChildTile != null)
        //        {
        //            downSTile = downChildTile.parent;
        //        }
        //        if (downSTile != null && downSTile.walkability == 1)
        //        {
        //            near.Add(Node.Create(node.X, node.Y - 1));
        //        }

        //        return near;
        //    }
        //}
    }
}

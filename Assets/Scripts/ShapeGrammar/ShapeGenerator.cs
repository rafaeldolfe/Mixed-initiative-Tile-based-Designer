using MissionGrammar;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ShapeGrammar
{
    public class ShapeGenerator : MonoBehaviour
    {
        [Serializable]
        public class RoomTilePairing
        {
            public RoomType roomType;
            public Tile tile;
        }

        [Serializable]
        public class MissionPriorityPairing
        {
            public MissionName missionName;

            /// <summary>
            /// The higher the priority, the faster the room type is
            /// chosen to be generated out of all other subordinates of a node.
            /// </summary>
            public int priority;
        }

        private MissionGenerator mg;

        private Cell[,] grid = new Cell[Constants.GRAPH_GRID_WIDTH, Constants.GRAPH_GRID_HEIGHT];
        private List<Cell> generatedSpaces = new List<Cell>();
        private Dictionary<MissionNode, List<Cell>> missionToGeneratedSpace = new Dictionary<MissionNode, List<Cell>>();
        private Dictionary<MissionNode, List<(int, Cell)>> missionToRankedGeneratedSpace = new Dictionary<MissionNode, List<(int, Cell)>>();
        private EditorCoroutine editorCoroutine;
        private List<SpaceState> replayStates;

        public List<SpaceRuleSetting> ruleSettings;
        public List<RoomTilePairing> roomTilePairings;
        public List<MissionPriorityPairing> roomPriorityPairings;
        /// <summary>
        /// This is a quick fix.
        /// </summary>
        public LevelGenerator levelGen;
        public GameObject container;
        public GameObject spaceRulePrefab;
        public GameObject connectionSprite;
        public Tilemap tiles;
        public Tilemap wallsRight;
        public Tilemap wallsUp;
        public Tilemap wallsLeft;
        public Tilemap wallsDown;
        public Tile wallRight;
        public Tile wallUp;
        public Tile wallLeft;
        public Tile wallDown;

        [Button]
        public SpaceState GenerateShape(bool visualize = true)
        {
            Reset();
            mg = FindObjectOfType(typeof(MissionGenerator)) as MissionGenerator;
            LoadRules();
            EditorApplication.ExecuteMenuItem("File/Save Project");

            MissionGraph graph = mg.GenerateMission(visualize);

            GenerateShape(graph, visualize);

            SpaceState finalState = replayStates.Last();

            if (visualize)
            {
                VisualizeGrammar(finalState);
            }

            return finalState;
        }

        public float secondsBetweenReplayStates;

        [Button]
        public void Replay()
        {
            StopReplay();
            if (replayStates == null)
            {
                Debug.LogWarning($"You must generate space first to save a replay of it");
            }
            {
                editorCoroutine = EditorCoroutineUtility.StartCoroutine(ReplayGeneration(replayStates), this);
            }
        }

        [Button]
        public void StopReplay()
        {
            if (editorCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(editorCoroutine);
            }
        }

        private IEnumerator ReplayGeneration(List<SpaceState> states)
        {
            foreach (SpaceState state in states)
            {
                VisualizeGrammar(state);
                yield return new EditorWaitForSeconds(secondsBetweenReplayStates);
            }
            yield return null;
        }

        private void GenerateShape(MissionGraph graph, bool visualize = true)
        {
            List<ShapeRule> rules = ruleSettings
                .Where(setting => setting.activated)
                .Select(setting => setting.rule.GetPlainClass())
                .ToList();

            List<ShapeRule> rulesApplied = new List<ShapeRule>();

            List<SpaceState> states = new List<SpaceState>();

            MissionExecutionHelper helper = MissionExecutionHelper.GetSequence(graph, roomPriorityPairings);

            int i = 0;
            while (helper.sequence.Count != 0 && i < 1000)
            {
                MissionNode current = helper.sequence.Dequeue();

                helper.nodeToTightCouplingDict.TryGetValue(current, out MissionNode tightSuperordinate);

                ShapeRule ruleApplied = ApplyRandomSpaceRule(current, rules, tightSuperordinate);

                states.Add(CopyState());

                if (ruleApplied == null)
                {
                    Debug.LogWarning($"Mission {current.missionName} failed to apply");
                }
                else
                {
                    if (ruleApplied.missionName == MissionName.Nothing)
                    {
                        for (int _ = 0; _ < Constants.MISSION_NOTHING_REPEAT_NR; _++)
                        {
                            ApplySpaceRule(current, ruleApplied, tightSuperordinate);
                            states.Add(CopyState());
                        }
                    }
                    rulesApplied.Add(ruleApplied);
                }
                i++;
            }
            replayStates = states;

            if (visualize)
            {
                VisualizeRulesApplied(rulesApplied);
            }
        }

        private SpaceState CopyState()
        {
            return new SpaceState { space = generatedSpaces.Select(space => space.DeepCopy()).ToList() };
        }

        private void VisualizeGrammar(SpaceState state)
        {
            Transform connectionsTransform = container.transform.Find("Connections");
            if (connectionsTransform == null)
            {
                connectionsTransform = new GameObject().transform;
                connectionsTransform.name = "Connections";
                connectionsTransform.transform.SetParent(container.transform);
            }
            int i = 0;
            while (connectionsTransform.childCount > 0 && i < 1000)
            {
                DestroyImmediate(connectionsTransform.transform.GetChild(0).gameObject);
                i++;
            }
            tiles.ClearAllTiles();
            wallsRight.ClearAllTiles();
            wallsUp.ClearAllTiles();
            wallsLeft.ClearAllTiles();
            wallsDown.ClearAllTiles();
            foreach (Cell cell in state.space)
            {
                RoomTilePairing finalPairing = roomTilePairings.Where(pairing => pairing.roomType == cell.roomType).First();
                Vector3Int cellPosition = new Vector3Int(cell.x - Constants.GRAPH_GRID_WIDTH / 2, cell.y - Constants.GRAPH_GRID_HEIGHT / 2, 0);
                tiles.SetTile(cellPosition, finalPairing.tile);
                wallsRight.SetTile(cellPosition, wallRight);
                wallsUp.SetTile(cellPosition, wallUp);
                wallsLeft.SetTile(cellPosition, wallLeft);
                wallsDown.SetTile(cellPosition, wallDown);

                foreach (Direction direction in cell.connections)
                {
                    switch (direction)
                    {
                        case Direction.RIGHT:
                            wallsRight.SetTile(cellPosition, null);
                            break;

                        case Direction.UP:
                            wallsUp.SetTile(cellPosition, null);
                            break;

                        case Direction.LEFT:
                            wallsLeft.SetTile(cellPosition, null);
                            break;

                        case Direction.DOWN:
                            wallsDown.SetTile(cellPosition, null);
                            break;

                        default:
                            break;
                    }
                }

                foreach (Direction direction in cell.possibleConnections)
                {
                    GameObject clone = Instantiate(connectionSprite, connectionsTransform);
                    clone.name = $"Connection-{direction}";
                    clone.transform.position = new Vector3(cell.x - Constants.GRAPH_GRID_WIDTH / 2, cell.y - Constants.GRAPH_GRID_HEIGHT / 2);
                    switch (direction)
                    {
                        case Direction.RIGHT:
                            clone.transform.position += new Vector3(0.5f, 0);
                            break;

                        case Direction.DOWN:
                            clone.transform.position += new Vector3(0, -0.5f);
                            break;

                        case Direction.LEFT:
                            clone.transform.position += new Vector3(-0.5f, 0);
                            break;

                        case Direction.UP:
                            clone.transform.position += new Vector3(0, 0.5f);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private void VisualizeRulesApplied(List<ShapeRule> rulesApplied)
        {
            Transform rulesAppliedTransform = container.transform.Find("RulesApplied");
            if (rulesAppliedTransform == null)
            {
                rulesAppliedTransform = Instantiate(spaceRulePrefab, container.transform).transform;
                rulesAppliedTransform.name = "RulesApplied";
            }
            int i = 0;
            while (rulesAppliedTransform.childCount > 0 && i < 1000)
            {
                DestroyImmediate(rulesAppliedTransform.transform.GetChild(0).gameObject);
                i++;
            }

            foreach (ShapeRule rule in rulesApplied)
            {
                GameObject clone = Instantiate(spaceRulePrefab, rulesAppliedTransform);
                clone.name = rule.ToString();
            }

            container.ExpandRecursive(true);
        }

        private ShapeRule ApplyRandomSpaceRule(MissionNode node, List<ShapeRule> rules, MissionNode tightSuperordinate = null)
        {
            List<ShapeRule> filteredRules = rules
                .Where(r => r.missionName == node.missionName)
                .ToList();

            if (filteredRules.Count == 0)
            {
                return null;
            }

            ShapeRule rule = filteredRules.PickRandom();

            return ApplySpaceRule(node, rule, tightSuperordinate);
        }

        private ShapeRule ApplySpaceRule(MissionNode node, ShapeRule rule, MissionNode tightSuperordinate)
        {
            List<Connection> connections;
            List<Connection> allConnections = GetAllConnections();
            List<Connection> tightConnections = new List<Connection>();
            List<Cell> tightSuperordinateSpaceCells = null;
            if (tightSuperordinate != null)
            {
                List<(int, Cell)> prioritizedCells = missionToRankedGeneratedSpace[tightSuperordinate];
                prioritizedCells.Sort((e1, e2) => e1.Item1.CompareTo(e2.Item1));
                tightSuperordinateSpaceCells = missionToRankedGeneratedSpace[tightSuperordinate].Select(space => space.Item2).ToList();
                tightConnections = GetConnections(tightSuperordinateSpaceCells);
            }

            connections = tightSuperordinate == null ? allConnections : tightConnections;

            Connection connection;

            // Generate space randomized among specific connections

            while (connections.Count != 0)
            {
                connection = connections.PickRandom();
                connections.Remove(connection);
                if (GenerateSpace(connection, rule, node, tightSuperordinate))
                {
                    return rule;
                }
            }

            // Generate space in more randomized connections
            while ((connection = SpawnRandomConnection(tightSuperordinateSpaceCells)) != null)
            {
                if (connection == null)
                {
                    throw new Exception($"Completely out of new places to spawn connection This should be impossible.");
                }
                GenerateSpace(connection, rule, node, tightSuperordinate);
                return rule;
            }
            string tightString = tightSuperordinate != null ? $" The tight superordinate was a {tightSuperordinate.missionName}." : "";
            Debug.LogWarning($"Failed to substitute {(tightSuperordinate != null ? "=>" : "->")} " +
                $"mission {node.missionName} with {rule.roomTemplatePositions.Count} " +
                $"number of space rule tiles. {tightString}");
            return null;
        }

        private bool GenerateSpace(Connection connection, ShapeRule rule, MissionNode node, MissionNode tightSuperordinate)
        {
            Cell connectionLocationCell = grid[connection.location.x, connection.location.y];
            if (connectionLocationCell != null && connectionLocationCell.roomType == RoomType.Goal)
            {
                return false;
            }
            Vector2Int target = connection.GetConnectionTarget();
            Cell rightCell = grid[target.x + 1, target.y];
            Cell upCell = grid[target.x, target.y + 1];
            Cell leftCell = grid[target.x - 1, target.y];
            Cell downCell = grid[target.x, target.y - 1];
            if ((node.missionName == MissionName.Lock || node.missionName == MissionName.LockMulti) 
                && rightCell != null 
                && upCell != null 
                && leftCell != null 
                && downCell != null)
            {
                return false;
            }
            List<SpaceRuleTile> tiles = GetTransformedSpaceRuleTiles(connection.location, connection.direction, rule);
            if (tiles != null)
            {
                GenerateCells(tiles, node.specialFunctionalities);
                BindCells(connection);
                missionToGeneratedSpace[node] = tiles
                    .Select(tile => grid[tile.position.x, tile.position.y]).ToList();
                missionToRankedGeneratedSpace[node] = tiles
                    .Select(tile => (0, grid[tile.position.x, tile.position.y])).ToList();
                if (tightSuperordinate != null)
                {
                    missionToGeneratedSpace[tightSuperordinate]
                        .AddRange(tiles.Select(tile => grid[tile.position.x, tile.position.y]));
                    missionToRankedGeneratedSpace[tightSuperordinate]
                        .AddRange(tiles.Select(tile => (missionToRankedGeneratedSpace.Count, grid[tile.position.x, tile.position.y])));
                }
                RemoveConnectionsIntoGeneratedSpace(tiles.Select(tile => tile.position).ToList());
                RemoveInvalidConnectionsOutOfGeneratedSpace(tiles.Select(tile => tile.position).ToList());
                return true;
            }
            return false;
        }

        private void GenerateCells(List<SpaceRuleTile> tiles, List<SpecialFunctionality> specialFunctionalities)
        {
            foreach (SpaceRuleTile tile in tiles)
            {
                Vector2Int location = tile.position;

                Cell cell = new Cell
                {
                    parentGrid = grid,
                    roomType = tile.roomType,
                    x = location.x,
                    y = location.y,
                    specialFunctionalities = specialFunctionalities,
                    possibleConnections = new List<Direction>(tile.possibleDirections),

                };
                grid[location.x, location.y] = cell;
                generatedSpaces.Add(cell);
            }
        }

        private void BindCells(Connection connection)
        {
            Cell parent = grid[connection.location.x, connection.location.y];
            Cell child = grid[connection.GetConnectionTarget().x, connection.GetConnectionTarget().y];

            if (parent == null)
            {
                return;
            }

            if (parent.roomType == RoomType.Goal)
            {
                if (parent.connections.Count > 0)
                {
                    throw new Exception($"Something went awfully wrong");
                }
            }

            parent.connections.Add(connection.direction);
            child.connections.Add(connection.direction.GetOppositeDirection());
        }

        private Connection SpawnRandomConnection(List<Cell> list)
        {
            Connection connection = SpawnRandomConnectionInSpace(list);

            if (connection == null)
            {
                // Failed to produce connection in tight superordinate space,
                // proceed to produce connection randomly across all generated space.

                connection = SpawnRandomConnectionInSpace(generatedSpaces);
            }

            return connection;
        }

        private void RemoveInvalidConnectionsOutOfGeneratedSpace(List<Vector2Int> locations)
        {
            foreach (Vector2Int location in locations)
            {
                Cell cell = grid[location.x, location.y];

                RemoveInvalidConnectionsOutOfGeneratedCell(cell);
            }
        }

        private void RemoveInvalidConnectionsOutOfGeneratedCell(Cell cell)
        {
            for (int i = 0; i < cell.possibleConnections.Count; i++)
            {
                Direction possibleDirection = cell.possibleConnections[i];

                switch (possibleDirection)
                {
                    case Direction.RIGHT:
                        if (grid[cell.x + 1, cell.y] != null)
                        {
                            cell.possibleConnections.RemoveAt(i);
                            i--;
                        }
                        break;

                    case Direction.UP:
                        if (grid[cell.x, cell.y + 1] != null)
                        {
                            cell.possibleConnections.RemoveAt(i);
                            i--;
                        }
                        break;

                    case Direction.LEFT:
                        if (grid[cell.x - 1, cell.y] != null)
                        {
                            cell.possibleConnections.RemoveAt(i);
                            i--;
                        }
                        break;

                    case Direction.DOWN:
                        if (grid[cell.x, cell.y - 1] != null)
                        {
                            cell.possibleConnections.RemoveAt(i);
                            i--;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void RemoveConnectionsIntoGeneratedSpace(List<Vector2Int> locations)
        {
            foreach (Vector2Int location in locations)
            {
                Cell cell = grid[location.x, location.y];

                RemoveConnectionsIntoGeneratedCell(cell);
            }
        }

        private void RemoveConnectionsIntoGeneratedCell(Cell cell)
        {
            Cell current = grid[cell.x - 1, cell.y];
            if (current != null)
            {
                current.possibleConnections.Remove(Direction.RIGHT);
            }
            current = grid[cell.x, cell.y - 1];
            if (current != null)
            {
                current.possibleConnections.Remove(Direction.UP);
            }
            current = grid[cell.x + 1, cell.y];
            if (current != null)
            {
                current.possibleConnections.Remove(Direction.LEFT);
            }
            current = grid[cell.x, cell.y + 1];
            if (current != null)
            {
                current.possibleConnections.Remove(Direction.DOWN);
            }
        }

        /// <summary>
        /// Returns null if the transformed space rule tiles are already occupied.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="direction"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private List<SpaceRuleTile> GetTransformedSpaceRuleTiles(Vector2Int location, Direction direction, ShapeRule rule)
        {
            int x = location.x;
            int y = location.y;

            List<SpaceRuleTile> newSpaceRuleTiles = new List<SpaceRuleTile>();

            foreach (SpaceRuleTile tile in rule.roomTemplatePositions)
            {
                int newX = location.x;
                int newY = location.y;
                if (direction == Direction.RIGHT)
                {
                    newX += tile.position.x;
                    newY += tile.position.y;

                    // Move to the root tile of the connection
                    newX += 1;
                }
                else if (direction == Direction.UP)
                {
                    newX += (-1) * tile.position.y;
                    newY += tile.position.x;

                    // Move to the root tile of the connection
                    newY += 1;
                }
                else if (direction == Direction.LEFT)
                {
                    newX += (-1) * tile.position.x;
                    newY += (-1) * tile.position.y;

                    // Move to the root tile of the connection
                    newX -= 1;
                }
                else if (direction == Direction.DOWN)
                {
                    newX += tile.position.y;
                    newY += (-1) * tile.position.x;

                    // Move to the root tile of the connection
                    newY -= 1;
                }

                Vector2Int newPosition = new Vector2Int(newX, newY);

                List<Direction> newDirections = tile.possibleDirections
                    .Select(possibleDirection => possibleDirection.TransformDirection(direction))
                    .ToList();

                SpaceRuleTile newTile = new SpaceRuleTile
                {
                    position = newPosition,
                    roomType = tile.roomType,
                    possibleDirections = newDirections
                };

                newSpaceRuleTiles.Add(newTile);
            }

            foreach (SpaceRuleTile tile in newSpaceRuleTiles)
            {
                if (generatedSpaces.Contains(grid[tile.position.x, tile.position.y]))
                {
                    return null;
                }
            }
            return newSpaceRuleTiles;
        }

        private List<Connection> GetConnections(List<Cell> tightSuperordinateSpace)
        {
            return tightSuperordinateSpace.SelectMany(cell => cell.GetConnections()).ToList();
        }

        private Connection SpawnRandomConnectionInSpace(List<Cell> tightSuperordinateSpace)
        {
            tightSuperordinateSpace.Shuffle();
            foreach (Cell cell in tightSuperordinateSpace)
            {
                cell.randomSpawnCount++;
                Connection randomNewConnection = SpawnRandomConnectionAtCell(cell);
                if (randomNewConnection != null)
                {
                    return randomNewConnection;
                }
            }
            return null;
        }

        private Connection SpawnRandomConnectionAtCell(Cell cell)
        {
            if (cell.roomType == RoomType.Goal)
            {
                return null;
            }
            List<Direction> directions = GetDirections();
            directions = directions.Shuffle();
            foreach (Direction direction in directions)
            {
                Cell adjacentCell = cell;
                switch (direction)
                {
                    case Direction.RIGHT:
                        adjacentCell = grid[cell.x + 1, cell.y];
                        break;

                    case Direction.DOWN:
                        adjacentCell = grid[cell.x, cell.y - 1];
                        break;

                    case Direction.LEFT:
                        adjacentCell = grid[cell.x - 1, cell.y];
                        break;

                    case Direction.UP:
                        adjacentCell = grid[cell.x, cell.y + 1];
                        break;

                    default:
                        break;
                }
                if (adjacentCell == null)
                {
                    List<Direction> connectionsToCheck = new List<Direction>(cell.connections);
                    connectionsToCheck.Add(direction);
                    if (levelGen.roomTypeToDictionaryDict.ContainsKey(cell.roomType))
                    {
                        if (!levelGen.roomTypeToDictionaryDict[cell.roomType].ContainsKey(connectionsToCheck.GetRoomShape()))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    cell.possibleConnections.Add(direction);
                    return new Connection(direction, new Vector2Int(cell.x, cell.y));
                }
            }
            return null;
        }

        private List<Direction> GetDirections()
        {
            return new List<Direction> { Direction.RIGHT, Direction.UP, Direction.LEFT, Direction.DOWN };
        }

        private List<Connection> GetAllConnections()
        {
            if (generatedSpaces.Count == 0)
            {
                return new List<Connection>
            {
                new Connection(Direction.RIGHT, new Vector2Int(Constants.GRAPH_GRID_WIDTH / 2, Constants.GRAPH_GRID_HEIGHT / 2))
            };
            }
            else
            {
                var where = generatedSpaces.Where(cell => cell.possibleConnections.Count != 0).ToList();
                var selectMany = where.SelectMany(cell => cell.GetConnections()).ToList();
                return selectMany;
            }
        }

        private void OnValidate()
        {
            LoadRules();
        }

        private void Reset()
        {
            StopReplay();
            grid = new Cell[Constants.GRAPH_GRID_WIDTH, Constants.GRAPH_GRID_HEIGHT];
            generatedSpaces = new List<Cell>();
            missionToGeneratedSpace = new Dictionary<MissionNode, List<Cell>>();
            missionToRankedGeneratedSpace = new Dictionary<MissionNode, List<(int, Cell)>>();
        }

        private void LoadRules()
        {
            string resourcePath = Constants.SPACE_RULES_PATH.ToResourcePath();
            SpaceRuleScriptableObject[] temp = Resources.LoadAll<SpaceRuleScriptableObject>(resourcePath);
            ruleSettings = ruleSettings.Where(setting => setting.rule != null).ToList();
            foreach (SpaceRuleScriptableObject rule in temp)
            {
                if (!ruleSettings.Select(setting => setting.rule).Contains(rule))
                {
                    ruleSettings = temp.Select(tempRule =>
                    {
                        SpaceRuleSetting tempSetting = ruleSettings.Find(setting => setting.rule == tempRule);
                        bool activated = tempSetting != null ? tempSetting.activated : true;
                        return new SpaceRuleSetting(tempRule, activated);
                    }).ToList();
                    return;
                }
            }
        }

        private class MissionExecutionHelper
        {
            public Queue<MissionNode> sequence;
            public Dictionary<MissionNode, MissionNode> nodeToTightCouplingDict;

            public static MissionExecutionHelper GetSequence(MissionGraph graph, List<MissionPriorityPairing> priorities)
            {
                Queue<MissionNode> sequence = new Queue<MissionNode>();
                Dictionary<MissionNode, MissionNode> nodeToTightCouplingDict = new Dictionary<MissionNode, MissionNode>();

                GetSequenceRecursive(graph.root, sequence, nodeToTightCouplingDict, priorities);

                return new MissionExecutionHelper { sequence = sequence, nodeToTightCouplingDict = nodeToTightCouplingDict };
            }

            private static void GetSequenceRecursive(MissionNode current, Queue<MissionNode> sequence, Dictionary<MissionNode, MissionNode> nodeToTightCouplingDict, List<MissionPriorityPairing> priorities)
            {
                sequence.Enqueue(current);

                List<MissionNode> tightSubordinates = current.subordinateTightCouplings;
                tightSubordinates.Shuffle();
                SortByPriorities(tightSubordinates, priorities);
                List<MissionNode> subordinates = current.subordinateNodes;
                subordinates.Shuffle();
                SortByPriorities(subordinates, priorities);

                foreach (MissionNode tightSubordinate in tightSubordinates)
                {
                    nodeToTightCouplingDict[tightSubordinate] = current;
                    bool allSuperordinatesHaveBeenVisited = CheckIfAllSuperordinatesHaveBeenVisited(tightSubordinate, sequence.ToList());
                    if (allSuperordinatesHaveBeenVisited)
                    {
                        GetSequenceRecursive(tightSubordinate, sequence, nodeToTightCouplingDict, priorities);
                    }
                }
                foreach (MissionNode subordinate in subordinates)
                {
                    bool allSuperordinatesHaveBeenVisited = CheckIfAllSuperordinatesHaveBeenVisited(subordinate, sequence.ToList());
                    if (allSuperordinatesHaveBeenVisited)
                    {
                        GetSequenceRecursive(subordinate, sequence, nodeToTightCouplingDict, priorities);
                    }
                }
            }

            private static bool CheckIfAllSuperordinatesHaveBeenVisited(MissionNode node, List<MissionNode> visited)
            {
                bool allSuperordinatesHaveBeenVisited = true;
                foreach (MissionNode otherSuperordinate in node.superordinateNodes)
                {
                    if (!visited.Contains(otherSuperordinate))
                    {
                        allSuperordinatesHaveBeenVisited = false;
                    }
                }
                if (node.superordinateTightCoupling != null && !visited.Contains(node.superordinateTightCoupling))
                {
                    allSuperordinatesHaveBeenVisited = false;
                }
                return allSuperordinatesHaveBeenVisited;
            }

            private static void SortByPriorities(List<MissionNode> nodes, List<MissionPriorityPairing> pairings)
            {
                Dictionary<MissionName, int> prioritiesDict = new Dictionary<MissionName, int>();
                foreach (MissionName name in Enum.GetValues(typeof(MissionName)))
                {
                    prioritiesDict[name] = 0;
                }
                foreach (MissionPriorityPairing pairing in pairings)
                {
                    prioritiesDict[pairing.missionName] = pairing.priority;
                }
                nodes.Sort((ic1, ic2) => prioritiesDict[ic2.missionName].CompareTo(prioritiesDict[ic1.missionName]));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MissionGrammar;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

internal static class ExtensionMethodsClass
{
    public static List<T> Shuffle<T>(this List<T> list)
    {
        Random rand = new();
        return list.OrderBy(item => rand.Next()).ToList();
    }

    public static T PickRandom<T>(this List<T> source)
    {
        return source[UnityEngine.Random.Range(0, source.Count())];
    }

    public static MissionNode GetFirstMatch(this List<MissionNode> nodes, MissionNode match)
    {
        return nodes.Find(node => node.isTerminal == match.isTerminal && node.missionName == match.missionName);
    }

    /// <summary>
    ///     Returns null if a complete match between the lists cannot be found.
    /// </summary>
    /// <param name="ruleNode"></param>
    /// <param name="graphNode"></param>
    /// <returns></returns>
    public static List<MissionNode> GetFirstCompleteMatch(this List<MissionNode> ruleNodes,
        List<MissionNode> graphNodes)
    {
        List<MissionNode> graphNodeLeavesCopy = new(graphNodes);
        List<MissionNode> result = new();
        foreach (MissionNode node in ruleNodes)
        {
            MissionNode match = graphNodeLeavesCopy.GetFirstMatch(node);
            if (match == null)
            {
                return null;
            }

            graphNodeLeavesCopy.Remove(match);
            result.Add(match);
        }

        return result;
    }

    public static bool CheckCompleteMatch(this MissionNode ruleNodes, MissionNode graphNodes)
    {
        List<MissionNode> graphNodeSubordinatesCopy = new(graphNodes.subordinateNodes);
        List<MissionNode> graphNodeTightSubordinatesCopy = new(graphNodes.subordinateTightCouplings);
        List<MissionNode> result = new();
        foreach (MissionNode node in ruleNodes.subordinateNodes)
        {
            MissionNode match = graphNodeSubordinatesCopy.GetFirstMatch(node);
            if (match == null)
            {
                return false;
            }

            graphNodeSubordinatesCopy.Remove(match);
            result.Add(match);
        }

        foreach (MissionNode node in ruleNodes.subordinateTightCouplings)
        {
            MissionNode match = graphNodeTightSubordinatesCopy.GetFirstMatch(node);
            if (match == null)
            {
                return false;
            }

            graphNodeTightSubordinatesCopy.Remove(match);
            result.Add(match);
        }

        return true;
    }

    public static string ToResourcePath(this string path)
    {
        return path.Substring(path.IndexOf("Resources/") + "Resources/".Length);
    }

    public static string ToUnityPath(this string path)
    {
        return path.Substring(path.IndexOf("Assets"));
    }

    public static T PopAt<T>(this List<T> list, int index)
    {
        T r = list[index];
        list.RemoveAt(index);
        return r;
    }

    public static Direction TransformDirection(this Direction self, Direction transformant)
    {
        int offset = 0;
        if (transformant == Direction.RIGHT)
        {
            offset = 0;
        }
        else if (transformant == Direction.UP)
        {
            offset = 1;
        }
        else if (transformant == Direction.LEFT)
        {
            offset = 2;
        }
        else if (transformant == Direction.DOWN)
        {
            offset = 3;
        }

        int newDirection = mod((int)self + offset, 4);
        return (Direction)newDirection;
    }

    public static void ExpandRecursive(this GameObject go, bool expand)
    {
        Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        MethodInfo methodInfo = type.GetMethod("SetExpandedRecursive");

        EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        EditorWindow window = EditorWindow.focusedWindow;

        methodInfo.Invoke(window, new object[] {go.GetInstanceID(), expand});
    }

    public static Direction GetOppositeDirection(this Direction direction)
    {
        switch (direction)
        {
            case Direction.RIGHT:
                return Direction.LEFT;

            case Direction.UP:
                return Direction.DOWN;

            case Direction.LEFT:
                return Direction.RIGHT;

            case Direction.DOWN:
                return Direction.UP;

            default:
                throw new Exception($"direction {direction} had no opposite");
        }
    }

    public static RoomShape GetRoomShape(this List<Direction> directions)
    {
        int east = directions.Contains(Direction.RIGHT) ? 1 : 0;
        int south = directions.Contains(Direction.DOWN) ? 1 : 0;
        int west = directions.Contains(Direction.LEFT) ? 1 : 0;
        int north = directions.Contains(Direction.UP) ? 1 : 0;
        if (east + south + west + north == 4)
        {
            return RoomShape.FourPiece;
        }

        if (east + south + west + north == 3)
        {
            return RoomShape.ThreePiece;
        }

        if (east + south + west + north == 2)
        {
            if (east + west == 2 || south + north == 2)
            {
                return RoomShape.TwoPieceStraight;
            }

            return RoomShape.TwoPieceTurn;
        }

        if (east + south + west + north == 1)
        {
            return RoomShape.OnePiece;
        }

        return RoomShape.None;
    }

    private static int mod(int x, int m)
    {
        return ((x % m) + m) % m;
    }
}
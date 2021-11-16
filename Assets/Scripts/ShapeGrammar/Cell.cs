using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShapeGrammar
{
    public class Cell
    {
        public Cell[,] parentGrid;
        public int x;
        public int y;
        public RoomType roomType;
        public List<SpecialFunctionality> specialFunctionalities;
        public List<Direction> connections = new List<Direction>();
        public List<Direction> possibleConnections = new List<Direction>();
        public int randomSpawnCount = 0;

        public List<Connection> GetConnections()
        {
            return possibleConnections.Select(p => new Connection(p, new Vector2Int(x, y))).ToList();
        }

        internal Cell DeepCopy()
        {
            Cell copy = this.MemberwiseClone() as Cell;
            copy.possibleConnections = new List<Direction>(possibleConnections);
            copy.connections = new List<Direction>(connections);
            return copy;
        }

        public override string ToString()
        {
            return $"({x},{y}) {roomType},{connections.GetRoomShape()}";
        }
    }
}

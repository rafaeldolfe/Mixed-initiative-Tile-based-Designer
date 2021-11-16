using Priority_Queue;
using UnityEngine;

namespace ThreadedPathfinding
{
    public class Node : FastPriorityQueueNode
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public static Node Create(int x, int y)
        {
            // Unfortunately, I can't find any reasonable way to implement pooling.
            // It would have to work accross multiple threads at one, and more imporantly, still inherit from FastPriorityQueueNode and not break the HSPQ system.
            return new Node(x, y);
        }

        private Node(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector2(Node pn)
        {
            return new Vector2(pn.X, pn.Y);
        }

        public static explicit operator Vector2Int(Node pn)
        {
            return new Vector2Int(pn.X, pn.Y);
        }

        public override bool Equals(object obj)
        {
            var other = (Node)obj;
            return this.X == other.X && this.Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X + Y * 7;
        }

        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ")";
        }
    }
}

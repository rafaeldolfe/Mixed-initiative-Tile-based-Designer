using System;
using UnityEngine;

namespace ShapeGrammar
{
    [Serializable]
    public class Connection
    {
        public Direction direction;
        public Vector2Int location;

        public Connection(Direction direction, Vector2Int location)
        {
            this.direction = direction;
            this.location = location;
        }

        internal Connection DeepCopy()
        {
            return this.MemberwiseClone() as Connection;
        }

        public Vector2Int GetConnectionTarget()
        {
            switch (direction)
            {
                case Direction.RIGHT:
                    return new Vector2Int(location.x + 1, location.y);

                case Direction.UP:
                    return new Vector2Int(location.x, location.y + 1);

                case Direction.LEFT:
                    return new Vector2Int(location.x - 1, location.y);

                case Direction.DOWN:
                    return new Vector2Int(location.x, location.y - 1);

                default:
                    throw new Exception($"Directions enum is not exhaustive");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShapeGrammar
{
    [Serializable]
    public class SpaceRuleTile
    {
        public Vector2Int position;
        public RoomType roomType;
        public List<Direction> possibleDirections;

        public SpaceRuleTile DeepCopy()
        {
            SpaceRuleTile copy = this.MemberwiseClone() as SpaceRuleTile;
            copy.possibleDirections = new List<Direction>(possibleDirections);
            return copy;
        }
    }
}

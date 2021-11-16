using MissionGrammar;
using System;
using System.Collections.Generic;

namespace ShapeGrammar
{
    [Serializable]
    public class ShapeRule
    {
        public MissionName missionName;
        public List<SpaceRuleTile> roomTemplatePositions = new List<SpaceRuleTile>();

        public override string ToString()
        {
            string res = "";
            res += missionName;
            if (roomTemplatePositions.Count == 0)
            {
                return res;
            }
            res += "->";
            foreach (SpaceRuleTile tile in roomTemplatePositions)
            {
                if (char.IsLetter(res[res.Length - 1]))
                {
                    res += ",";
                }
                res += $"{tile.roomType}";
            }
            return res;
        }
    }
}

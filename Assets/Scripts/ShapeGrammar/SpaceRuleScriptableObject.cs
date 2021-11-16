using MissionGrammar;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShapeGrammar
{
    [Serializable]
    [CreateAssetMenu]
    public class SpaceRuleScriptableObject : ScriptableObject
    {
        public MissionName missionName;
        public List<SpaceRuleTile> roomTemplatePositions;

        public ShapeRule GetPlainClass()
        {
            return new ShapeRule { missionName = missionName, roomTemplatePositions = roomTemplatePositions };
        }
    }
}
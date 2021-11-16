using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MissionGrammar
{
    [Serializable]
    [CreateAssetMenu]
    public class MissionRuleScriptableObject : ScriptableObject
    {
        public bool isOriginalRule = true;
        [Title("Condition")]
        [HideLabel]
        [InlineProperty]
        public FlatMissionGraph condition;

        [Title("Result")]
        [HideLabel]
        [InlineProperty]
        public FlatMissionGraph result;

        internal MissionRule GetPlainClass()
        {
            var rule = new MissionRule
            {
                condition = condition.GetPlainClass(),
                result = result.GetPlainClass()
            };
            return rule;
        }
    }
}

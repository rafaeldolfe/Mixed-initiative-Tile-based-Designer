using Sirenix.OdinInspector;
using System;

namespace MissionGrammar
{
    [Serializable]
    public class MissionRuleSettings
    {
        public MissionRuleScriptableObject rule;
        public bool activated;
        [ShowIf("@activated")]
        public int weight = 1;

        public MissionRuleSettings(MissionRuleScriptableObject rule, bool activated)
        {
            this.rule = rule;
            this.activated = activated;
        }
    }
}

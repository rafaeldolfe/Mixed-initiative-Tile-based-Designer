using System;

namespace ShapeGrammar
{
    [Serializable]
    public class SpaceRuleSetting
    {
        public SpaceRuleScriptableObject rule;
        public bool activated;

        public SpaceRuleSetting(SpaceRuleScriptableObject rule, bool activated)
        {
            this.rule = rule;
            this.activated = activated;
        }
    }
}
using System;
using System.Collections.Generic;

namespace MissionGrammar
{
    [Serializable]
    public class MissionRule
    {
        public MissionGraph condition;
        public MissionGraph result;

        internal List<MissionNode> GetValidNodes(MissionGraph graph)
        {
            List<MissionNode> validNodes = new List<MissionNode>();
            foreach (MissionNode node in graph.nonTerminalNodes)
            {
                if (node.Matches(condition.root))
                {
                    if (DoSubTreesOfNodesMatch(condition.root, node))
                    {
                        validNodes.Add(node);
                    }
                }
            }
            return validNodes;
        }

        /// <summary>
        /// Currently only supports rule conditions of max depth = 2
        /// </summary>
        /// <param name="ruleNode"></param>
        /// <param name="graphNode"></param>
        /// <returns></returns>
        private bool DoSubTreesOfNodesMatch(MissionNode ruleNode, MissionNode graphNode)
        {
            return ruleNode.CheckCompleteMatch(graphNode);
        }

        public override string ToString()
        {
            return $"{CreateGraphString(condition.root, "")}->{CreateGraphString(result.root, "")}";
        }
        private string CreateGraphString(MissionNode node, string res)
        {
            string newRes = $"{res}{node.missionName.ToString()},";
            foreach (MissionNode subordinate in node.subordinateTightCouplings)
            {
                newRes = CreateGraphString(subordinate, newRes);
            }
            foreach (MissionNode subordinate in node.subordinateNodes)
            {
                newRes = CreateGraphString(subordinate, newRes);
            }
            return newRes;
        }

        public MissionRule DeepCopy()
        {
            MissionRule copy = new MissionRule
            {
                condition = condition.DeepCopy(),
                result = result.DeepCopy()
            };
            return copy;
        }
    }
}

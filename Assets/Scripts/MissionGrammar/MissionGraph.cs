using System;
using System.Collections.Generic;
using System.Linq;

namespace MissionGrammar
{
    [Serializable]
    public class MissionGraph
    {
        public MissionNode root;

        [NonSerialized]
        public List<MissionNode> nonTerminalNodes = new List<MissionNode>();

        internal MissionGraph DeepCopy()
        {
            Dictionary<int, MissionNode> intToNodeDict = new Dictionary<int, MissionNode>();
            Dictionary<MissionNode, int> nodeToIntDict = new Dictionary<MissionNode, int>();
            Dictionary<int, List<MissionNode>> intToSubordinates = new Dictionary<int, List<MissionNode>>();
            Dictionary<int, List<MissionNode>> intToTightSubordinates = new Dictionary<int, List<MissionNode>>();

            GetReferences(root, intToNodeDict, nodeToIntDict, intToSubordinates, intToTightSubordinates);

            Dictionary<int, MissionNode> copiedNodes = new Dictionary<int, MissionNode>();

            foreach (KeyValuePair<int, MissionNode> pair in intToNodeDict)
            {
                MissionNode copy = new MissionNode(pair.Value.isTerminal, pair.Value.missionName, pair.Value.ruleApplicationNumber, pair.Value.specialFunctionalities);
                copiedNodes[pair.Key] = copy;
            }
            foreach (KeyValuePair<int, MissionNode> pair in copiedNodes)
            {
                List<int> subordinates = intToSubordinates[pair.Key].Select(sub => nodeToIntDict[sub]).ToList();
                foreach (int subordinate in subordinates)
                {
                    pair.Value.SetSubordinateNode(copiedNodes[subordinate]);
                }
            }
            foreach (KeyValuePair<int, MissionNode> pair in copiedNodes)
            {
                List<int> subordinates = intToTightSubordinates[pair.Key].Select(sub => nodeToIntDict[sub]).ToList();
                foreach (int subordinate in subordinates)
                {
                    pair.Value.SetSubordinateTightCoupling(copiedNodes[subordinate]);
                }
            }
            var graph = new MissionGraph { root = copiedNodes[0] };
            graph.ComputeTerminalNodes();
            counterId = 0;
            return graph;
        }

        private int counterId = 0;

        public void GetReferences(MissionNode node, Dictionary<int, MissionNode> intToNodeDict, Dictionary<MissionNode, int> nodeToIntDict, Dictionary<int, List<MissionNode>> intToSubordinates, Dictionary<int, List<MissionNode>> intToTightSubordinates)
        {
            intToNodeDict[counterId] = node;
            nodeToIntDict[node] = counterId;
            intToSubordinates[counterId] = node.subordinateNodes;
            intToTightSubordinates[counterId] = node.subordinateTightCouplings;
            counterId++;
            foreach (MissionNode subordinate in node.subordinateTightCouplings)
            {
                GetReferences(subordinate, intToNodeDict, nodeToIntDict, intToSubordinates, intToTightSubordinates);
            }
            foreach (MissionNode subordinate in node.subordinateNodes)
            {
                GetReferences(subordinate, intToNodeDict, nodeToIntDict, intToSubordinates, intToTightSubordinates);
            }
        }

        public void ComputeTerminalNodes()
        {
            nonTerminalNodes.Clear();
            AddToNonTerminalsRecursive(root);
        }

        private void RemoveNonTerminalsRecursive(MissionNode node)
        {
            if (!node.isTerminal)
            {
                nonTerminalNodes.Remove(node);
            }
            foreach (MissionNode current in node.subordinateNodes)
            {
                RemoveNonTerminalsRecursive(current);
            }
            foreach (MissionNode current in node.subordinateTightCouplings)
            {
                RemoveNonTerminalsRecursive(current);
            }
        }

        private void AddToNonTerminalsRecursive(MissionNode node)
        {
            if (!node.isTerminal)
            {
                nonTerminalNodes.Add(node);
            }
            foreach (MissionNode current in node.subordinateNodes)
            {
                AddToNonTerminalsRecursive(current);
            }
            foreach (MissionNode current in node.subordinateTightCouplings)
            {
                AddToNonTerminalsRecursive(current);
            }
        }

        public static MissionGraph CreateMissionGraph()
        {
            var graph = new MissionGraph();

            graph.root = new MissionNode(true, MissionName.Root);
            MissionNode start = new MissionNode(false, MissionName.Start);
            graph.root.SetSubordinateNode(start);
            graph.nonTerminalNodes.Add(start);
            return graph;
        }

        public MissionGraph()
        {
        }

        //public MissionGraph(MissionNode root)
        //{
        //    this.root = root;
        //    if (!root.isTerminal)
        //    {
        //        nonTerminalNodes.Add(root);
        //    }
        //}

        internal void ApplyRule(MissionRule finalRule, MissionNode targetRoot)
        {
            MissionRule copiedFinalRule = finalRule.DeepCopy();
            MissionNode ruleRoot = copiedFinalRule.condition.root;

            List<MissionNode> ruleSubordinates = ruleRoot.subordinateNodes;
            List<MissionNode> targetSubordinates = ruleSubordinates.GetFirstCompleteMatch(targetRoot.subordinateNodes);

            List<MissionNode> ruleTightSubordinates = ruleRoot.subordinateTightCouplings;
            List<MissionNode> targetTightSubordinates = ruleTightSubordinates.GetFirstCompleteMatch(targetRoot.subordinateTightCouplings);

            List<MissionNode> allRuleNodes = new List<MissionNode> { ruleRoot };
            allRuleNodes.AddRange(ruleSubordinates != null ? ruleSubordinates : new List<MissionNode>());
            allRuleNodes.AddRange(ruleTightSubordinates != null ? ruleTightSubordinates : new List<MissionNode>());
            List<MissionNode> allTargetNodes = new List<MissionNode> { targetRoot };
            allTargetNodes.AddRange(targetSubordinates != null ? targetSubordinates : new List<MissionNode>());
            allTargetNodes.AddRange(targetTightSubordinates != null ? targetTightSubordinates : new List<MissionNode>());

            if (allRuleNodes.Count != allTargetNodes.Count)
            {
                throw new Exception($"Invalid logic");
            }
            targetRoot.ruleApplicationNumber = ruleRoot.ruleApplicationNumber;
            ApplyIdentification(ruleSubordinates, targetSubordinates);
            ApplyIdentification(ruleTightSubordinates, targetTightSubordinates);

            helper.Reset();

            helper.SaveAndRemoveEdgesRecursive(targetRoot, allTargetNodes);

            RemoveNonTerminalsRecursive(targetRoot);

            MissionNode resultRoot = copiedFinalRule.result.root;

            AddToNonTerminalsRecursive(resultRoot);

            helper.ApplyEdgesRecursive(resultRoot, new Dictionary<MissionNode, bool>());
        }

        private void ApplyIdentification(List<MissionNode> ruleSubordinates, List<MissionNode> targetSubordinates)
        {
            foreach (MissionNode ruleSub in ruleSubordinates)
            {
                targetSubordinates.Find(targetSub => targetSub.Matches(ruleSub)).ruleApplicationNumber = ruleSub.ruleApplicationNumber;
            }
        }

        private RuleSubstitutionHelper helper = new RuleSubstitutionHelper();

        private class RuleSubstitutionHelper
        {
            public Dictionary<int, List<MissionNode>> edgesInDict = new Dictionary<int, List<MissionNode>>();
            public Dictionary<int, List<MissionNode>> edgesOutDict = new Dictionary<int, List<MissionNode>>();

            public Dictionary<int, MissionNode> tightEdgesInDict = new Dictionary<int, MissionNode>();
            public Dictionary<int, List<MissionNode>> tightEdgesOutDict = new Dictionary<int, List<MissionNode>>();

            public void SaveAndRemoveEdgesRecursive(MissionNode node, List<MissionNode> ignoredNodes)
            {
                SaveAndRemoveEdges(node, ignoredNodes);
                SaveAndRemoveTightEdges(node, ignoredNodes);
                for (int i = 0; i < node.subordinateTightCouplings.Count; i++)
                {
                    MissionNode subordinate = node.subordinateTightCouplings[i];
                    if (!ignoredNodes.Contains(subordinate))
                    {
                        continue;
                    }
                    SaveAndRemoveEdgesRecursive(subordinate, ignoredNodes);
                }
                for (int i = 0; i < node.subordinateNodes.Count; i++)
                {
                    MissionNode subordinate = node.subordinateNodes[i];
                    if (!ignoredNodes.Contains(subordinate))
                    {
                        continue;
                    }
                    SaveAndRemoveEdgesRecursive(subordinate, ignoredNodes);
                }
            }

            private void SaveAndRemoveTightEdges(MissionNode node, List<MissionNode> ignoredNodes)
            {
                tightEdgesInDict[node.ruleApplicationNumber] = null;
                if (node.superordinateTightCoupling != null)
                {
                    if (ignoredNodes.Contains(node.superordinateTightCoupling))
                    {
                        // continue
                    }
                    else
                    {
                        tightEdgesInDict[node.ruleApplicationNumber] = node.superordinateTightCoupling;

                        node.superordinateTightCoupling.subordinateTightCouplings.Remove(node);
                        node.superordinateTightCoupling = null;
                    }
                }

                int length;
                tightEdgesOutDict[node.ruleApplicationNumber] = new List<MissionNode>();
                length = node.subordinateTightCouplings.Count - 1;
                for (int i = length; i > -1; i--)
                {
                    MissionNode tightSubordinate = node.subordinateTightCouplings[i];
                    if (ignoredNodes.Contains(tightSubordinate))
                    {
                        continue;
                    }
                    tightEdgesOutDict[node.ruleApplicationNumber].Add(tightSubordinate);
                    if (tightSubordinate.superordinateTightCoupling == null)
                    {
                        throw new Exception($"Logic error, tight subordinate has null superordinate tight coupling");
                    }
                    tightSubordinate.superordinateTightCoupling = null;
                    node.subordinateTightCouplings.RemoveAt(i);
                }
            }

            private void SaveAndRemoveEdges(MissionNode node, List<MissionNode> ignoredNodes)
            {
                int length;
                edgesInDict[node.ruleApplicationNumber] = new List<MissionNode>();
                length = node.superordinateNodes.Count - 1;
                for (int i = length; i > -1; i--)
                {
                    MissionNode superordinate = node.superordinateNodes[i];
                    if (ignoredNodes.Contains(superordinate))
                    {
                        continue;
                    }
                    edgesInDict[node.ruleApplicationNumber].Add(superordinate);
                    superordinate.subordinateNodes.Remove(node);
                    node.superordinateNodes.RemoveAt(i);
                }
                edgesOutDict[node.ruleApplicationNumber] = new List<MissionNode>();
                length = node.subordinateNodes.Count - 1;
                for (int i = length; i > -1; i--)
                {
                    MissionNode subordinate = node.subordinateNodes[i];
                    if (ignoredNodes.Contains(subordinate))
                    {
                        continue;
                    }
                    edgesOutDict[node.ruleApplicationNumber].Add(subordinate);
                    subordinate.superordinateNodes.Remove(node);
                    node.subordinateNodes.RemoveAt(i);
                }
            }

            public void ApplyEdgesRecursive(MissionNode node, Dictionary<MissionNode, bool> visited)
            {
                if (visited.ContainsKey(node) && visited[node])
                {
                    return;
                }
                visited[node] = true;

                List<MissionNode> totalSubordinates = ApplyEdges(node);
                totalSubordinates.AddRange(ApplyTightEdges(node));

                for (int i = 0; i < totalSubordinates.Count; i++)
                {
                    MissionNode subordinate = totalSubordinates[i];
                    ApplyEdgesRecursive(subordinate, visited);
                }
            }

            private List<MissionNode> ApplyEdges(MissionNode node)
            {
                List<MissionNode> copyPreSubordinates = new List<MissionNode>(node.subordinateNodes);
                if (edgesInDict.ContainsKey(node.ruleApplicationNumber) && edgesInDict[node.ruleApplicationNumber] != null && edgesInDict[node.ruleApplicationNumber].Count != 0)
                {
                    List<MissionNode> superordinates = edgesInDict[node.ruleApplicationNumber];
                    for (int i = 0; i < superordinates.Count; i++)
                    {
                        MissionNode superordinate = superordinates[i];
                        superordinate.SetSubordinateNode(node);
                    }
                }
                if (edgesOutDict.ContainsKey(node.ruleApplicationNumber) && edgesOutDict[node.ruleApplicationNumber] != null && edgesOutDict[node.ruleApplicationNumber].Count != 0)
                {
                    List<MissionNode> subordinates = edgesOutDict[node.ruleApplicationNumber];
                    for (int i = 0; i < subordinates.Count; i++)
                    {
                        MissionNode subordinate = subordinates[i];
                        node.SetSubordinateNode(subordinate);
                    }
                }
                return copyPreSubordinates;
            }

            private List<MissionNode> ApplyTightEdges(MissionNode node)
            {
                List<MissionNode> copyPreTightSubordinates = new List<MissionNode>(node.subordinateTightCouplings);
                if (tightEdgesInDict.ContainsKey(node.ruleApplicationNumber) && tightEdgesInDict[node.ruleApplicationNumber] != null)
                {
                    MissionNode superordinate = tightEdgesInDict[node.ruleApplicationNumber];
                    superordinate.SetSubordinateTightCoupling(node);
                }
                if (tightEdgesOutDict.ContainsKey(node.ruleApplicationNumber) && tightEdgesOutDict[node.ruleApplicationNumber] != null && tightEdgesOutDict[node.ruleApplicationNumber].Count != 0)
                {
                    List<MissionNode> subordinates = tightEdgesOutDict[node.ruleApplicationNumber];
                    for (int i = 0; i < subordinates.Count; i++)
                    {
                        MissionNode subordinate = subordinates[i];
                        node.SetSubordinateTightCoupling(subordinate);
                    }
                }
                return copyPreTightSubordinates;
            }

            internal void Reset()
            {
                edgesInDict.Clear();
                edgesOutDict.Clear();
                tightEdgesInDict.Clear();
                tightEdgesOutDict.Clear();
            }
        }
    }
}

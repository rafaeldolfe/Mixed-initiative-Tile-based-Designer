using System;
using System.Collections.Generic;

namespace MissionGrammar
{
    [Serializable]
    public class FlatMissionGraph
    {
        public List<FlatMissionNode> nodes = new List<FlatMissionNode>();

        internal MissionGraph GetPlainClass()
        {
            Dictionary<string, MissionNode> referencesDict = new Dictionary<string, MissionNode>();
            foreach (FlatMissionNode flatNode in nodes)
            {
                if (flatNode.ruleApplicationNumber == -1)
                {
                    throw new Exception($"Invalid logic, rule application number is -1");
                }
                if (flatNode.id == "")
                {
                    //throw new Exception($"Invalid logic, id is empty");
                }
                referencesDict[flatNode.id] = new MissionNode(flatNode.isTerminal, flatNode.type, flatNode.ruleApplicationNumber);
            }
            List<MissionNode> nonTerminalNodes = new List<MissionNode>();
            foreach (FlatMissionNode flatNode in nodes)
            {
                MissionNode node = referencesDict[flatNode.id];
                if (!node.isTerminal)
                {
                    nonTerminalNodes.Add(node);
                }
                if (flatNode.subordinates.Count != 0)
                {
                    foreach (DirectedEdge connection in flatNode.subordinates)
                    {
                        if (connection.isTightCoupling)
                        {
                            node.SetSubordinateTightCoupling(referencesDict[connection.pointsTo], connection.relationship);
                        }
                        else
                        {
                            node.SetSubordinateNode(referencesDict[connection.pointsTo], connection.relationship);
                        }
                    }
                }
            }
            if (nodes.Count == 0)
            {
                throw new Exception($"Logic error, graph was empty");
            }
            MissionGraph graph = new MissionGraph
            {
                root = referencesDict[nodes[0].id],
                nonTerminalNodes = nonTerminalNodes,
            };
            //graph.RepairGraph();
            return graph;
        }
    }
}
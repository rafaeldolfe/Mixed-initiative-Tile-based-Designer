using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MissionGrammar
{
    public class MissionGenerator : MonoBehaviour
    {
        public List<MissionRuleSettings> ruleSettings;

        public GameObject nodePrefab;
        public GameObject container;

        [Button]
        public MissionGraph GenerateMission(bool visualize = true)
        {
            LoadRules();
            EditorApplication.ExecuteMenuItem("File/Save Project");
            MissionGraph graph = MissionGraph.CreateMissionGraph();

            List<MissionRuleSettings> rules = ruleSettings.Where(setting => setting.activated).ToList();
            Dictionary<MissionRule, float> weightedRules = new Dictionary<MissionRule, float>();
            foreach (MissionRuleSettings item in rules)
            {
                weightedRules[item.rule.GetPlainClass()] = item.weight;
            }

            MissionRule current = ApplyRandomRuleOnGraph(graph, weightedRules);
            List<MissionRule> rulesApplied = new List<MissionRule> { };
            int i = 0;
            while (current != null && i < 1000)
            {
                rulesApplied.Add(current);
                current = ApplyRandomRuleOnGraph(graph, weightedRules);
                i++;
            }
            if (visualize)
            {
                VisualizeGraph(graph, rulesApplied);
            }
            // Always skip the root and set its first subordinate node as root 
            graph.root = graph.root.subordinateNodes[0];
            return graph;
        }

        private void OnValidate()
        {
            LoadRules();
        }

        private void LoadRules()
        {
            string resourcePath = Constants.MISSION_RULES_PATH.ToResourcePath();
            MissionRuleScriptableObject[] temp = Resources.LoadAll<MissionRuleScriptableObject>(resourcePath);
            foreach (MissionRuleScriptableObject rule in temp)
            {
                if (!ruleSettings.Select(setting => setting.rule).Contains(rule))
                {
                    ruleSettings = temp.Select(tempRule => new MissionRuleSettings(tempRule, tempRule.isOriginalRule ? true : false)).ToList();
                    return;
                }
            }
        }

        private void VisualizeGraph(MissionGraph graph, List<MissionRule> rulesApplied)
        {
            int i = 0;
            while (container.transform.childCount > 0 && i < 1000)
            {
                DestroyImmediate(container.transform.GetChild(0).gameObject);
                i++;
            }

            MissionNode current = graph.root;

            MakeTreeRecursive(container.transform, current, false, new Dictionary<MissionNode, bool>());

            GameObject rules = Instantiate(nodePrefab, container.transform);
            rules.name = "RulesApplied";
            foreach (MissionRule rule in rulesApplied)
            {
                GameObject clone = Instantiate(nodePrefab, rules.transform);
                clone.name = rule.ToString();
            }

            SetExpandedRecursive(container, true);
        }

        private void MakeTreeRecursive(Transform parent, MissionNode parentNode, bool isTightlyCoupledWithParent, Dictionary<MissionNode, bool> builtBefore)
        {
            if (builtBefore.ContainsKey(parentNode))
            {
                if (!parent.name.Contains("references"))
                {
                    parent.name = $"{parent.name} (references: {parentNode.missionName})";
                }
                else
                {
                    parent.name = $"{parent.name} (and more)";
                }
                return;
            }
            builtBefore[parentNode] = true;
            GameObject childGameObject = Instantiate(nodePrefab, container.transform);
            childGameObject.name = isTightlyCoupledWithParent ? "=>" : "->";
            childGameObject.name = builtBefore.Count == 1 ? "" : childGameObject.name;
            childGameObject.name = $"{childGameObject.name}{parentNode.missionName}";
            childGameObject.transform.SetParent(parent);

            foreach (MissionNode node in parentNode.subordinateNodes)
            {
                MakeTreeRecursive(childGameObject.transform, node, false, builtBefore);
            }
            foreach (MissionNode node in parentNode.subordinateTightCouplings)
            {
                MakeTreeRecursive(childGameObject.transform, node, true, builtBefore);
            }
        }

        private static void SetExpandedRecursive(GameObject go, bool expand)
        {
            Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            MethodInfo methodInfo = type.GetMethod("SetExpandedRecursive");

            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            EditorWindow window = EditorWindow.focusedWindow;

            methodInfo.Invoke(window, new object[] { go.GetInstanceID(), expand });
        }

        /// <summary>
        /// Applies a random rule on the given graph.
        /// Returns null if no rules can be applied.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        private MissionRule ApplyRandomRuleOnGraph(MissionGraph graph, Dictionary<MissionRule, float> weightedRules)
        {
            Dictionary<(MissionRule, List<MissionNode>), float> possibleWeightedRules = new Dictionary<(MissionRule, List<MissionNode>), float>();
            foreach (KeyValuePair<MissionRule, float> rule in weightedRules)
            {
                List<MissionNode> nodes = rule.Key.GetValidNodes(graph);
                if (nodes.Count > 0)
                {
                    possibleWeightedRules[(rule.Key, nodes)] = rule.Value;
                }
            }
            if (possibleWeightedRules.Count == 0)
            {
                return null;
            }
            (MissionRule, List<MissionNode>) possibility = WeightedRandomizer.From(possibleWeightedRules).TakeOne();
            MissionRule finalRule = possibility.Item1;
            MissionNode finalTargetNode = possibility.Item2.PickRandom();

            graph.ApplyRule(finalRule, finalTargetNode);

            return finalRule;
        }
    }
}

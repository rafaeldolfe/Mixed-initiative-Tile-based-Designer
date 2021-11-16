using System;
using System.Collections.Generic;
namespace MissionGrammar
{
    [Serializable]
    public class FlatMissionNode
    {
        public bool isTerminal;
        public MissionName type;
        public int ruleApplicationNumber = -1;
        public string id = "";
        public List<DirectedEdge> subordinates;
    }
}

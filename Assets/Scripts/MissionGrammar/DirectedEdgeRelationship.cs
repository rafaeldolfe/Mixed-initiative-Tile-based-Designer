using System;

namespace MissionGrammar
{
    [Serializable]
    public class DirectedEdgeRelationship
    {
        public SpecialNodeType start;
        public SpecialNodeType end;
        public string id;
    }
}
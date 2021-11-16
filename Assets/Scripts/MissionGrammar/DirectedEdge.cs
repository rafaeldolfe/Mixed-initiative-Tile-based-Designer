using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MissionGrammar
{
    [Serializable]
    public class DirectedEdge
    {
        public string pointsTo;
        public bool isTightCoupling;
        [HideInInspector]
        public DirectedEdgeRelationship relationship;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
namespace MissionGrammar
{

    [Serializable]
    public class MissionNode
    {
        public bool isTerminal;
        public MissionName missionName;
        public int ruleApplicationNumber;
        public List<MissionNode> superordinateNodes = new List<MissionNode>();
        public List<MissionNode> subordinateNodes = new List<MissionNode>();
        public MissionNode superordinateTightCoupling;
        public List<MissionNode> subordinateTightCouplings = new List<MissionNode>();
        public List<SpecialFunctionality> specialFunctionalities = new List<SpecialFunctionality>();

        public MissionNode(bool isTerminal, MissionName type)
        {
            this.isTerminal = isTerminal;
            this.missionName = type;
        }

        public MissionNode(bool isTerminal, MissionName type, int ruleApplicationNumber)
        {
            this.isTerminal = isTerminal;
            this.missionName = type;
            this.ruleApplicationNumber = ruleApplicationNumber;
        }

        public MissionNode(bool isTerminal, MissionName type, int ruleApplicationNumber, List<SpecialFunctionality> specialFunctionalities)
        {
            this.isTerminal = isTerminal;
            this.missionName = type;
            this.ruleApplicationNumber = ruleApplicationNumber;
            this.specialFunctionalities = specialFunctionalities.Select(specialFunctionality => specialFunctionality.DeepCopy()).ToList();
        }
        public void SetSubordinateNode(MissionNode subordinate)
        {
            subordinateNodes.Add(subordinate);
            subordinate.superordinateNodes.Add(this);
        }
        public void SetSubordinateTightCoupling(MissionNode subordinate)
        {
            subordinateTightCouplings.Add(subordinate);
            subordinate.SetSuperordinateTightCoupling(this);
        }
        public void SetSubordinateNode(MissionNode subordinate, DirectedEdgeRelationship relationship)
        {
            subordinateNodes.Add(subordinate);
            subordinate.superordinateNodes.Add(this);

            SetSpecialFunctionality(subordinate, relationship);
        }

        public void SetSubordinateTightCoupling(MissionNode subordinate, DirectedEdgeRelationship relationship)
        {
            subordinateTightCouplings.Add(subordinate);
            subordinate.SetSuperordinateTightCoupling(this);

            SetSpecialFunctionality(subordinate, relationship);
        }
        private void SetSpecialFunctionality(MissionNode subordinate, DirectedEdgeRelationship relationship)
        {
            if (relationship.start == SpecialNodeType.None || relationship.end == SpecialNodeType.None)
            {
                return;
            }
            SpecialFunctionality subFunctionality = subordinate.specialFunctionalities.Find(func => func.tempId == relationship.id && func.tempId != "");
            SpecialFunctionality selfFunctionality = specialFunctionalities.Find(func => func.tempId == relationship.id && func.tempId != "");

            if (subFunctionality == null && selfFunctionality == null)
            {
                string guid = Guid.NewGuid().ToString();
                SpecialFunctionality start = new SpecialFunctionality { guid = guid, type = relationship.start, tempId = relationship.id };
                SpecialFunctionality end = new SpecialFunctionality { guid = guid, type = relationship.end, tempId = relationship.id };
                subordinate.specialFunctionalities.Add(end);
                specialFunctionalities.Add(start);
            }
            else if (subFunctionality == null && selfFunctionality != null)
            {
                string existingGuid = selfFunctionality.guid;
                SpecialFunctionality end = new SpecialFunctionality { guid = existingGuid, type = relationship.end, tempId = relationship.id };
                specialFunctionalities.Add(end);
            }
            else if (subFunctionality != null && selfFunctionality == null)
            {
                string existingGuid = subFunctionality.guid;
                SpecialFunctionality start = new SpecialFunctionality { guid = existingGuid, type = relationship.start, tempId = relationship.id };
                specialFunctionalities.Add(start);
            }
            else
            {
                throw new Exception($"Something went wrong. Had already assigned relationship before with id {relationship.id}");
            }
        }
        private void SetSuperordinateTightCoupling(MissionNode superordinate)
        {
            if (superordinateTightCoupling != null)
            {
                throw new Exception($"Logic error, already had tight superordinate coupling");
            }
            superordinateTightCoupling = superordinate;
        }

        internal bool Matches(MissionNode match)
        {
            return isTerminal == match.isTerminal && missionName == match.missionName;
        }

        public override string ToString()
        {
            return $"{ruleApplicationNumber}:{missionName}";
        }
    }
}

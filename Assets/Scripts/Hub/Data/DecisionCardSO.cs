using UnityEngine;

namespace Hub
{
    public enum DecisionCardType
    {
        ConflictIntervention, // 분쟁 개입
        InternalIssue,        // 내부 문제
        ExternalProposal,     // 외부 제안
        IntelReport           // 정보 보고
    }

    [CreateAssetMenu(fileName = "DecisionCard", menuName = "Hub/Decision Card")]
    public class DecisionCardSO : ScriptableObject
    {
        public string cardTitle;
        public DecisionCardType cardType;

        [TextArea(3, 10)]
        public string description;

        [Header("Consequences")]
        [TextArea(2, 5)]
        public string ignoreConsequenceText;
        [TextArea(2, 5)]
        public string engageConsequenceText;

        [Header("Engage Effects")]
        public float stabilityChange;
        public float crueltyChange;
        public float trustChange;
        public float tensionChange;

        [Header("Resource Effects (on engage)")]
        public int personnelChange;
        public int informationChange;
        public int influenceChange;

        [Header("Ignore Effects")]
        public float ignoreStabilityChange;
        public float ignoreTensionChange;

        [Header("Priority")]
        [Range(1, 5)]
        public int urgencyLevel = 1;
    }
}

using System.Collections.Generic;

namespace Hub
{
    [System.Serializable]
    public class DayState
    {
        public int dayNumber;
        public List<DecisionCardSO> availableCards = new List<DecisionCardSO>();
        public List<DecisionCardSO> chosenCards = new List<DecisionCardSO>();
        public List<bool> engagedChoices = new List<bool>(); // true = engaged, false = ignored
        public int maxDecisionsPerDay = 3;

        public bool HasRemainingDecisions => chosenCards.Count < maxDecisionsPerDay && availableCards.Count > 0;
        public bool IsComplete => !HasRemainingDecisions;
    }
}

using System.Collections.Generic;
using UnityEngine;
using Core;

namespace Hub
{
    public class HubManager : MonoBehaviour
    {
        public static HubManager Instance { get; private set; }

        [Header("Card Pool")]
        [SerializeField] private DecisionCardSO[] allCards;

        [Header("Events")]
        [SerializeField] public GameEventSO onDayStarted;
        [SerializeField] public GameEventSO onDayConfirmed;
        [SerializeField] public GameEventSO onDecisionMade;

        public OrganizationData Organization { get; private set; } = new OrganizationData();
        public DayState CurrentDay { get; private set; }

        private List<UnitHubStatus> unitStatuses = new List<UnitHubStatus>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            StartNewDay();
        }

        public void StartNewDay()
        {
            int dayNumber = 1;
            if (TurnManager.Instance != null)
                dayNumber = TurnManager.Instance.GetTurnCount() + 1;

            CurrentDay = new DayState
            {
                dayNumber = dayNumber,
                maxDecisionsPerDay = 3
            };

            GenerateCards();
            RefreshUnitStatuses();
            onDayStarted?.Raise();
        }

        private void GenerateCards()
        {
            CurrentDay.availableCards.Clear();

            if (allCards == null || allCards.Length == 0)
            {
                // Generate placeholder cards if no SO assets assigned
                return;
            }

            // Pick 1-3 random cards based on world tension
            int cardCount = 1;
            float tension = HubResourceManager.Instance != null ? HubResourceManager.Instance.WorldTension : 10f;
            if (tension >= 40f) cardCount = 2;
            if (tension >= 70f) cardCount = 3;

            var shuffled = new List<DecisionCardSO>(allCards);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            for (int i = 0; i < Mathf.Min(cardCount, shuffled.Count); i++)
            {
                CurrentDay.availableCards.Add(shuffled[i]);
            }
        }

        public void MakeDecision(DecisionCardSO card, bool engage)
        {
            if (!CurrentDay.HasRemainingDecisions) return;
            if (!CurrentDay.availableCards.Contains(card)) return;

            CurrentDay.availableCards.Remove(card);
            CurrentDay.chosenCards.Add(card);
            CurrentDay.engagedChoices.Add(engage);

            // Apply immediate effects preview (actual execution on ConfirmDay)
            onDecisionMade?.Raise();
        }

        public void ConfirmDay()
        {
            // Apply all chosen card effects
            for (int i = 0; i < CurrentDay.chosenCards.Count; i++)
            {
                var card = CurrentDay.chosenCards[i];
                bool engaged = CurrentDay.engagedChoices[i];
                ApplyCardEffects(card, engaged);
            }

            // Tension increases slightly every day
            if (HubResourceManager.Instance != null)
                HubResourceManager.Instance.IncreaseWorldTension(Random.Range(1f, 4f));

            onDayConfirmed?.Raise();

            // Start next day
            StartNewDay();
        }

        private void ApplyCardEffects(DecisionCardSO card, bool engaged)
        {
            if (engaged)
            {
                Organization.stability += card.stabilityChange;
                Organization.crueltyIndex += card.crueltyChange;
                Organization.trustLevel += card.trustChange;

                Organization.stability = Mathf.Clamp(Organization.stability, 0f, 100f);
                Organization.crueltyIndex = Mathf.Clamp(Organization.crueltyIndex, 0f, 100f);
                Organization.trustLevel = Mathf.Clamp(Organization.trustLevel, 0f, 100f);

                if (HubResourceManager.Instance != null)
                {
                    if (card.personnelChange != 0) HubResourceManager.Instance.AddResource(HubResourceType.Personnel, card.personnelChange);
                    if (card.informationChange != 0) HubResourceManager.Instance.AddResource(HubResourceType.Information, card.informationChange);
                    if (card.influenceChange != 0) HubResourceManager.Instance.AddResource(HubResourceType.Influence, card.influenceChange);
                    HubResourceManager.Instance.IncreaseWorldTension(card.tensionChange);
                }

                if (card.cardType == DecisionCardType.ConflictIntervention)
                    Organization.forcedOperationsCount++;
            }
            else
            {
                // Ignore consequences
                Organization.stability += card.ignoreStabilityChange;
                Organization.stability = Mathf.Clamp(Organization.stability, 0f, 100f);

                if (HubResourceManager.Instance != null)
                    HubResourceManager.Instance.IncreaseWorldTension(card.ignoreTensionChange);
            }
        }

        public void RefreshUnitStatuses()
        {
            unitStatuses.Clear();
            if (GameManager.Instance == null) return;

            var units = GameManager.Instance.UnitManager.GetAlivePlayerUnits();
            foreach (var unit in units)
            {
                unitStatuses.Add(new UnitHubStatus(unit));
            }
        }

        public List<UnitHubStatus> GetUnitStatuses()
        {
            return unitStatuses;
        }

        public List<string> GetRelationshipWarnings()
        {
            // Placeholder - synergy/conflict detection not yet implemented
            var warnings = new List<string>();
            warnings.Add("No critical relationship issues detected.");
            return warnings;
        }

        public int GetDayNumber()
        {
            return CurrentDay != null ? CurrentDay.dayNumber : 1;
        }
    }
}

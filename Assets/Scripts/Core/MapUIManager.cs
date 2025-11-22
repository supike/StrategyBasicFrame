using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MapUIManager : MonoBehaviour
    {
        [SerializeField] private Button endTurnButton;
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private GameObject unitInfoPanel;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private Slider daySlider;

        void Start()
        {
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
            TurnManager.Instance.onTurnChanging.RegisterListener(OnChangingTurn);
            TurnManager.Instance.onPlayerTurnStart?.RegisterListener(OnPlayerTurnStart);
            TurnManager.Instance.onEnemyTurnStart?.RegisterListener(OnEnemyTurnStart);
        }

        void OnChangingTurn()
        {
            daySlider.value = TurnManager.Instance.GetTurnHour();
            turnText.text = "Turn " + TurnManager.Instance.GetTurnCount();
            // Optionally handle turn changing UI updates here
        }
        void OnPlayerTurnStart()
        {
            turnText.text = "Player Turn";
            endTurnButton.interactable = true;
        }

        void OnEnemyTurnStart()
        {
            turnText.text = "Enemy Turn";
            endTurnButton.interactable = false;
        }

        void OnEndTurnClicked()
        {
            TurnManager.Instance.EndTurn();
            
            endTurnButton.interactable = !endTurnButton.interactable;
        }

        public void ShowUnitInfo(Unit unit)
        {
            //unitInfoPanel.SetActive(true);
            unitNameText.text = unit.UnitName;
            daySlider.maxValue = 7;
            daySlider.value = unit.CurrentHealth;
        }

        public void HideUnitInfo()
        {
            unitInfoPanel.SetActive(false);
        }
    }
}
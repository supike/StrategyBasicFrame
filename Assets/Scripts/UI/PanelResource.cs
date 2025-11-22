using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PanelResource: MonoBehaviour
    {
        
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private TextMeshProUGUI humanText;
        [SerializeField] private TextMeshProUGUI endTurnButton;
        
        void Start()
        {
            UpdateResourceUI();
            ResourceManager.Instance.onResourceChanged.RegisterListener(UpdateResourceUI);
        }
        void UpdateResourceUI()
        {
            goldText.text = ResourceManager.Instance.GetResource(ResourceType.Gold).ToString();
            foodText.text = ResourceManager.Instance.GetResource(ResourceType.Food).ToString();
            humanText.text = ResourceManager.Instance.GetResource(ResourceType.Wood).ToString();
        }
        
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace Hub.UI
{
    /// <summary>
    /// Full-screen overlay modal that shows detailed unit information.
    /// Opened when clicking a unit in the Unit Tab.
    /// </summary>
    public class HubUnitDetailModal : MonoBehaviour
    {
        private static HubUnitDetailModal instance;

        private GameObject modalRoot;
        private Text nameText;
        private Text classText;
        private Text emotionText;
        private Text healthCondText;
        private Text healthValueText;
        private Text staminaValueText;
        private Text balanceValueText;
        private Text moraleValueText;
        private Text levelText;
        private Text modeText;
        private Image portraitImage;

        public static void Create(Canvas canvas)
        {
            if (instance != null) return;

            GameObject go = new GameObject("UnitDetailModal", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);

            instance = go.AddComponent<HubUnitDetailModal>();
            instance.BuildModal(go.GetComponent<RectTransform>());
            instance.Hide();
        }

        private void BuildModal(RectTransform root)
        {
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            modalRoot = root.gameObject;

            // Semi-transparent backdrop
            Image backdrop = root.gameObject.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.75f);
            backdrop.raycastTarget = true;

            // Add button to backdrop for closing on click outside
            Button backdropBtn = root.gameObject.AddComponent<Button>();
            backdropBtn.onClick.AddListener(Hide);
            ColorBlock colors = backdropBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            backdropBtn.colors = colors;

            // Center card
            RectTransform card = HubUIHelper.CreatePanel(root, "DetailCard",
                new Vector2(0.25f, 0.1f), new Vector2(0.75f, 0.9f),
                Vector2.zero, Vector2.zero,
                HubUIHelper.CardColor);
            card.GetComponent<Image>().raycastTarget = true;

            // Prevent clicks on card from closing modal
            Button cardBlock = card.gameObject.AddComponent<Button>();
            cardBlock.onClick.AddListener(() => { }); // swallow click
            ColorBlock cardColors = cardBlock.colors;
            cardColors.normalColor = Color.white;
            cardColors.highlightedColor = Color.white;
            cardColors.pressedColor = Color.white;
            cardColors.selectedColor = Color.white;
            cardBlock.colors = cardColors;

            // Close button (top-right)
            Button closeBtn = HubUIHelper.CreateButton(card, "X",
                HubUIHelper.DangerColor, Color.white, 18);
            RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.88f, 0.92f);
            closeBtnRect.anchorMax = new Vector2(0.98f, 0.99f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;
            closeBtn.onClick.AddListener(Hide);

            // Portrait area (left side)
            RectTransform portraitArea = HubUIHelper.CreatePanel(card, "PortraitArea",
                new Vector2(0.03f, 0.55f), new Vector2(0.35f, 0.90f),
                Vector2.zero, Vector2.zero,
                HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.05f));

            GameObject portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portraitGO.transform.SetParent(portraitArea, false);
            portraitImage = portraitGO.GetComponent<Image>();
            portraitImage.color = HubUIHelper.SubTextColor;
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
            RectTransform portraitRect = portraitGO.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.1f, 0.05f);
            portraitRect.anchorMax = new Vector2(0.9f, 0.95f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;

            // Name
            nameText = HubUIHelper.CreateText(card, "Unit Name", 22, TextAnchor.MiddleLeft, Color.white);
            nameText.fontStyle = FontStyle.Bold;
            nameText.rectTransform.anchorMin = new Vector2(0.38f, 0.82f);
            nameText.rectTransform.anchorMax = new Vector2(0.85f, 0.90f);
            nameText.rectTransform.offsetMin = Vector2.zero;
            nameText.rectTransform.offsetMax = Vector2.zero;

            // Class
            classText = HubUIHelper.CreateText(card, "Infantry", 14, TextAnchor.MiddleLeft, HubUIHelper.AccentColor);
            classText.rectTransform.anchorMin = new Vector2(0.38f, 0.76f);
            classText.rectTransform.anchorMax = new Vector2(0.85f, 0.82f);
            classText.rectTransform.offsetMin = Vector2.zero;
            classText.rectTransform.offsetMax = Vector2.zero;

            // Battle Mode
            modeText = HubUIHelper.CreateText(card, "Mode: Normal", 13, TextAnchor.MiddleLeft, HubUIHelper.SubTextColor);
            modeText.rectTransform.anchorMin = new Vector2(0.38f, 0.70f);
            modeText.rectTransform.anchorMax = new Vector2(0.85f, 0.76f);
            modeText.rectTransform.offsetMin = Vector2.zero;
            modeText.rectTransform.offsetMax = Vector2.zero;

            // Level
            levelText = HubUIHelper.CreateText(card, "Level 1  |  Exp: 0", 13, TextAnchor.MiddleLeft, HubUIHelper.SubTextColor);
            levelText.rectTransform.anchorMin = new Vector2(0.38f, 0.64f);
            levelText.rectTransform.anchorMax = new Vector2(0.85f, 0.70f);
            levelText.rectTransform.offsetMin = Vector2.zero;
            levelText.rectTransform.offsetMax = Vector2.zero;

            // Condition row
            emotionText = HubUIHelper.CreateText(card, "Emotion: Stable", 13, TextAnchor.MiddleLeft, HubUIHelper.SubTextColor);
            emotionText.rectTransform.anchorMin = new Vector2(0.38f, 0.58f);
            emotionText.rectTransform.anchorMax = new Vector2(0.65f, 0.64f);
            emotionText.rectTransform.offsetMin = Vector2.zero;
            emotionText.rectTransform.offsetMax = Vector2.zero;

            healthCondText = HubUIHelper.CreateText(card, "Health: Normal", 13, TextAnchor.MiddleLeft, HubUIHelper.SubTextColor);
            healthCondText.rectTransform.anchorMin = new Vector2(0.65f, 0.58f);
            healthCondText.rectTransform.anchorMax = new Vector2(0.97f, 0.64f);
            healthCondText.rectTransform.offsetMin = Vector2.zero;
            healthCondText.rectTransform.offsetMax = Vector2.zero;

            // Stats section
            RectTransform statsPanel = HubUIHelper.CreatePanel(card, "StatsPanel",
                new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.52f),
                new Vector2(8f, 8f), new Vector2(-8f, -8f),
                HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.03f));

            Text statsTitle = HubUIHelper.CreateText(statsPanel, "COMBAT STATS", 12, TextAnchor.UpperLeft, HubUIHelper.SubTextColor);
            statsTitle.fontStyle = FontStyle.Bold;
            statsTitle.rectTransform.anchorMin = new Vector2(0, 0.88f);
            statsTitle.rectTransform.anchorMax = new Vector2(1, 1);
            statsTitle.rectTransform.offsetMin = new Vector2(10f, 0f);
            statsTitle.rectTransform.offsetMax = new Vector2(-10f, -4f);

            // Stat bars
            float yStart = 0.78f;
            float yStep = 0.2f;

            healthValueText = BuildStatBar(statsPanel, "Health", "100 / 100", yStart, HubUIHelper.DangerColor, 1f);
            staminaValueText = BuildStatBar(statsPanel, "Stamina", "100 / 100", yStart - yStep, HubUIHelper.WarningColor, 1f);
            balanceValueText = BuildStatBar(statsPanel, "Balance", "100 / 100", yStart - yStep * 2, HubUIHelper.AccentColor, 1f);
            moraleValueText = BuildStatBar(statsPanel, "Morale", "50", yStart - yStep * 3, new Color(0.6f, 0.4f, 0.8f, 1f), 0.5f);
        }

        private Text BuildStatBar(RectTransform parent, string label, string defaultValue, float yAnchor, Color barColor, float fillAmount)
        {
            float rowHeight = 0.18f;

            Text labelText = HubUIHelper.CreateText(parent, label.ToUpperInvariant(), 11, TextAnchor.MiddleLeft, HubUIHelper.SubTextColor);
            labelText.rectTransform.anchorMin = new Vector2(0, yAnchor);
            labelText.rectTransform.anchorMax = new Vector2(0.2f, yAnchor + rowHeight);
            labelText.rectTransform.offsetMin = new Vector2(10f, 0f);
            labelText.rectTransform.offsetMax = new Vector2(-4f, 0f);

            HubUIHelper.CreateProgressBar(parent, label + "Bar",
                new Vector2(0.2f, yAnchor + 0.02f), new Vector2(0.75f, yAnchor + rowHeight - 0.02f),
                new Vector2(4f, 2f), new Vector2(-4f, -2f),
                new Color(0.06f, 0.07f, 0.1f, 1f), barColor, fillAmount);

            Text valueText = HubUIHelper.CreateText(parent, defaultValue, 12, TextAnchor.MiddleRight, Color.white);
            valueText.rectTransform.anchorMin = new Vector2(0.75f, yAnchor);
            valueText.rectTransform.anchorMax = new Vector2(1f, yAnchor + rowHeight);
            valueText.rectTransform.offsetMin = new Vector2(4f, 0f);
            valueText.rectTransform.offsetMax = new Vector2(-10f, 0f);

            return valueText;
        }

        public static void Show(Unit unit)
        {
            if (instance == null) return;
            instance.PopulateData(unit);
            instance.modalRoot.SetActive(true);
        }

        public static void Show(UnitHubStatus status)
        {
            if (status != null && status.unit != null)
                Show(status.unit);
        }

        private void PopulateData(Unit unit)
        {
            if (unit == null) return;

            string unitName = unit.unitData != null ? unit.unitData.characterName : "Unknown";
            nameText.text = unitName;

            classText.text = unit.unitData != null ? unit.unitData.unitclass.ToString() : "Unknown";
            modeText.text = "Mode: " + unit.stats.unitMode.ToString();
            levelText.text = $"Level {unit.stats.level}  |  Exp: {unit.stats.experience}";

            if (unit.unitData != null)
            {
                emotionText.text = "Emotion: " + unit.unitData.EmotionCondition.ToString();
                emotionText.color = HubUIHelper.GetEmotionColor(unit.unitData.EmotionCondition);

                healthCondText.text = "Health: " + unit.unitData.HealthCondition.ToString();

                if (unit.unitData.portraitImage != null)
                {
                    portraitImage.sprite = unit.unitData.portraitImage;
                    portraitImage.color = Color.white;
                }
            }

            healthValueText.text = $"{unit.CurrentHealth} / {unit.stats.maxHealth}";
            staminaValueText.text = $"{unit.CurrentStamina:F0} / {unit.stats.maxStamina}";
            balanceValueText.text = $"{unit.CurrentBalance:F0} / {unit.MaxBalance}";
            moraleValueText.text = unit.stats.morale.ToString();
        }

        public void Hide()
        {
            if (modalRoot != null)
                modalRoot.SetActive(false);
        }
    }
}

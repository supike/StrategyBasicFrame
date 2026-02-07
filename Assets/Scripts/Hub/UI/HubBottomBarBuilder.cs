using UnityEngine;
using UnityEngine.UI;

namespace Hub.UI
{
    /// <summary>
    /// Builds the bottom bar: choice summary chips + day confirmation button (FTL-style).
    /// </summary>
    public class HubBottomBarBuilder
    {
        private RectTransform chipContainer;
        private Button confirmButton;
        private Text confirmButtonText;

        public void Build(RectTransform parent)
        {
            // Choice summary area (left 70%)
            RectTransform summaryArea = HubUIHelper.CreatePanel(parent, "ChoiceSummary",
                new Vector2(0f, 0f), new Vector2(0.70f, 1f),
                new Vector2(12f, 8f), new Vector2(-8f, -8f),
                Color.clear);

            Text summaryLabel = HubUIHelper.CreateText(summaryArea, "TODAY'S CHOICES:", 11,
                TextAnchor.MiddleLeft, HubUIHelper.SubTextColor);
            summaryLabel.rectTransform.anchorMin = new Vector2(0, 0.5f);
            summaryLabel.rectTransform.anchorMax = new Vector2(0.18f, 1f);
            summaryLabel.rectTransform.offsetMin = new Vector2(4f, 0f);
            summaryLabel.rectTransform.offsetMax = new Vector2(-4f, -4f);

            // Chip container for selected decisions
            GameObject chipContainerGO = new GameObject("ChipContainer", typeof(RectTransform));
            chipContainerGO.transform.SetParent(summaryArea, false);

            chipContainer = chipContainerGO.GetComponent<RectTransform>();
            chipContainer.anchorMin = new Vector2(0.18f, 0.1f);
            chipContainer.anchorMax = new Vector2(1f, 0.9f);
            chipContainer.offsetMin = Vector2.zero;
            chipContainer.offsetMax = Vector2.zero;

            HorizontalLayoutGroup chipLayout = chipContainerGO.AddComponent<HorizontalLayoutGroup>();
            chipLayout.spacing = 8f;
            chipLayout.childAlignment = TextAnchor.MiddleLeft;
            chipLayout.childForceExpandWidth = false;
            chipLayout.childForceExpandHeight = true;
            chipLayout.padding = new RectOffset(8, 8, 4, 4);

            // Placeholder chip
            BuildChip(chipContainer, "No decisions yet", HubUIHelper.SubTextColor);

            // Confirm Day button (right 30%)
            RectTransform btnArea = HubUIHelper.CreatePanel(parent, "ConfirmArea",
                new Vector2(0.70f, 0f), new Vector2(1f, 1f),
                new Vector2(8f, 10f), new Vector2(-12f, -10f),
                Color.clear);

            confirmButton = HubUIHelper.CreateButton(btnArea, "CONFIRM DAY",
                HubUIHelper.ConfirmButtonColor, Color.white, 18);

            RectTransform btnRect = confirmButton.GetComponent<RectTransform>();
            btnRect.anchorMin = Vector2.zero;
            btnRect.anchorMax = Vector2.one;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            // Add a subtle border effect
            GameObject borderGO = new GameObject("ConfirmBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderGO.transform.SetParent(btnRect, false);

            Image borderImg = borderGO.GetComponent<Image>();
            borderImg.color = new Color(1f, 0.3f, 0.2f, 0.3f);
            borderImg.raycastTarget = false;

            RectTransform borderRect = borderGO.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-3f, -3f);
            borderRect.offsetMax = new Vector2(3f, 3f);
            borderGO.transform.SetAsFirstSibling();

            // Wire button
            if (Application.isPlaying)
            {
                confirmButton.onClick.AddListener(OnConfirmDay);
            }
        }

        private void BuildChip(RectTransform parent, string text, Color color)
        {
            GameObject chipGO = new GameObject("Chip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            chipGO.transform.SetParent(parent, false);

            Image chipBg = chipGO.GetComponent<Image>();
            chipBg.color = HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.08f);
            chipBg.raycastTarget = false;

            LayoutElement le = chipGO.AddComponent<LayoutElement>();
            le.minWidth = 120f;
            le.preferredWidth = 140f;

            Text chipText = HubUIHelper.CreateText(chipGO.transform, text, 11, TextAnchor.MiddleCenter, color);
            chipText.rectTransform.offsetMin = new Vector2(8f, 0f);
            chipText.rectTransform.offsetMax = new Vector2(-8f, 0f);
        }

        public void RefreshChips()
        {
            if (chipContainer == null) return;

            // Clear existing chips
            for (int i = chipContainer.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(chipContainer.GetChild(i).gameObject);
            }

            if (HubManager.Instance == null || HubManager.Instance.CurrentDay == null)
            {
                BuildChip(chipContainer, "No decisions yet", HubUIHelper.SubTextColor);
                return;
            }

            var day = HubManager.Instance.CurrentDay;
            if (day.chosenCards.Count == 0)
            {
                BuildChip(chipContainer, "No decisions yet", HubUIHelper.SubTextColor);
                return;
            }

            for (int i = 0; i < day.chosenCards.Count; i++)
            {
                var card = day.chosenCards[i];
                bool engaged = day.engagedChoices[i];
                string chipLabel = card.cardTitle + (engaged ? " [E]" : " [I]");
                Color chipColor = engaged ? HubUIHelper.AccentColor : HubUIHelper.WarningColor;
                BuildChip(chipContainer, chipLabel, chipColor);
            }
        }

        private void OnConfirmDay()
        {
            if (HubManager.Instance != null)
            {
                HubManager.Instance.ConfirmDay();
            }
        }
    }
}

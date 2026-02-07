using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hub.UI
{
    /// <summary>
    /// Builds the center panel with decision cards (1-3 per day).
    /// Each card forces a choice: Engage or Ignore.
    /// </summary>
    public class HubCenterPanelBuilder
    {
        private RectTransform cardContainer;
        private List<RectTransform> cardObjects = new List<RectTransform>();

        public void Build(RectTransform parent)
        {
            // Title area
            Text title = HubUIHelper.CreateText(parent, "TODAY'S DECISIONS", 20, TextAnchor.UpperCenter, HubUIHelper.TextColor);
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.anchorMin = new Vector2(0, 0.93f);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.offsetMin = new Vector2(20f, 0f);
            title.rectTransform.offsetMax = new Vector2(-20f, -8f);

            Text subtitle = HubUIHelper.CreateText(parent, "Choose wisely. Every decision has consequences.", 13, TextAnchor.UpperCenter, HubUIHelper.SubTextColor);
            subtitle.fontStyle = FontStyle.Italic;
            subtitle.rectTransform.anchorMin = new Vector2(0, 0.89f);
            subtitle.rectTransform.anchorMax = new Vector2(1, 0.93f);
            subtitle.rectTransform.offsetMin = new Vector2(20f, 0f);
            subtitle.rectTransform.offsetMax = new Vector2(-20f, 0f);

            // Card container with horizontal layout
            GameObject containerGO = new GameObject("CardContainer", typeof(RectTransform));
            containerGO.transform.SetParent(parent, false);

            cardContainer = containerGO.GetComponent<RectTransform>();
            cardContainer.anchorMin = new Vector2(0.02f, 0.02f);
            cardContainer.anchorMax = new Vector2(0.98f, 0.88f);
            cardContainer.offsetMin = Vector2.zero;
            cardContainer.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = containerGO.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(16, 16, 16, 16);

            // Build placeholder cards if in editor or if no HubManager
            if (!Application.isPlaying || HubManager.Instance == null)
            {
                BuildCard(cardContainer, "Regional Conflict Erupts",
                    DecisionCardType.ConflictIntervention,
                    "Armed conflict has broken out in the neighboring sector. Local militia requests immediate support.",
                    "If ignored:\n- Enemy influence +1\n- Civilian casualties increase",
                    "If engaged:\n- Combat initiated\n- Reward: Intel + Personnel\n- Risk: Casualties possible",
                    3);

                BuildCard(cardContainer, "Unit Morale Crisis",
                    DecisionCardType.InternalIssue,
                    "Several units are showing signs of extreme fatigue and low morale. Discipline is deteriorating.",
                    "If ignored:\n- Stability -5\n- Risk of desertion",
                    "If addressed:\n- Stability +3\n- Operations paused for 1 day",
                    2);

                BuildCard(cardContainer, "Intelligence Report",
                    DecisionCardType.IntelReport,
                    "Our scouts have intercepted enemy communications. The data suggests a major offensive within 3 days.",
                    "If ignored:\n- Information remains incomplete",
                    "If analyzed:\n- Information +2\n- Reveals future threat",
                    1);
            }
            else
            {
                var day = HubManager.Instance.CurrentDay;
                if (day != null)
                {
                    foreach (var card in day.availableCards)
                    {
                        BuildCard(cardContainer, card.cardTitle, card.cardType,
                            card.description, card.ignoreConsequenceText, card.engageConsequenceText,
                            card.urgencyLevel);
                    }
                }
            }
        }

        private void BuildCard(RectTransform parent, string title, DecisionCardType type,
            string description, string ignoreText, string engageText, int urgency)
        {
            // Card root
            GameObject cardGO = new GameObject("Card_" + title.Replace(" ", ""), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardGO.transform.SetParent(parent, false);

            Image cardBg = cardGO.GetComponent<Image>();
            cardBg.color = HubUIHelper.CardColor;
            cardBg.raycastTarget = false;

            RectTransform cardRect = cardGO.GetComponent<RectTransform>();
            cardObjects.Add(cardRect);

            // Type badge strip at top
            Color typeColor = HubUIHelper.GetCardTypeColor(type);
            RectTransform badge = HubUIHelper.CreatePanel(cardRect, "TypeBadge",
                new Vector2(0, 0.92f), new Vector2(1, 1),
                Vector2.zero, Vector2.zero, typeColor);

            Text badgeText = HubUIHelper.CreateText(badge, HubUIHelper.GetCardTypeLabel(type), 11,
                TextAnchor.MiddleCenter, Color.white);
            badgeText.fontStyle = FontStyle.Bold;

            // Urgency indicator
            string urgencyDots = new string('!', urgency);
            Text urgencyText = HubUIHelper.CreateText(badge, urgencyDots, 12, TextAnchor.MiddleRight, Color.white);
            urgencyText.rectTransform.offsetMax = new Vector2(-8f, 0f);

            // Title
            Text titleText = HubUIHelper.CreateText(cardRect, title, 16, TextAnchor.UpperLeft, Color.white);
            titleText.fontStyle = FontStyle.Bold;
            titleText.rectTransform.anchorMin = new Vector2(0, 0.78f);
            titleText.rectTransform.anchorMax = new Vector2(1, 0.92f);
            titleText.rectTransform.offsetMin = new Vector2(14f, 0f);
            titleText.rectTransform.offsetMax = new Vector2(-14f, -4f);

            // Description
            Text descText = HubUIHelper.CreateText(cardRect, description, 12, TextAnchor.UpperLeft, HubUIHelper.SubTextColor);
            descText.rectTransform.anchorMin = new Vector2(0, 0.55f);
            descText.rectTransform.anchorMax = new Vector2(1, 0.78f);
            descText.rectTransform.offsetMin = new Vector2(14f, 4f);
            descText.rectTransform.offsetMax = new Vector2(-14f, -4f);

            // Divider
            HubUIHelper.CreatePanel(cardRect, "Divider",
                new Vector2(0.05f, 0.54f), new Vector2(0.95f, 0.545f),
                Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.1f));

            // Ignore consequences (left side)
            Text ignoreLabel = HubUIHelper.CreateText(cardRect, ignoreText, 11, TextAnchor.UpperLeft, HubUIHelper.WarningColor);
            ignoreLabel.rectTransform.anchorMin = new Vector2(0, 0.25f);
            ignoreLabel.rectTransform.anchorMax = new Vector2(0.5f, 0.54f);
            ignoreLabel.rectTransform.offsetMin = new Vector2(14f, 0f);
            ignoreLabel.rectTransform.offsetMax = new Vector2(-6f, -6f);

            // Engage consequences (right side)
            Text engageLabel = HubUIHelper.CreateText(cardRect, engageText, 11, TextAnchor.UpperLeft, HubUIHelper.AccentColor);
            engageLabel.rectTransform.anchorMin = new Vector2(0.5f, 0.25f);
            engageLabel.rectTransform.anchorMax = new Vector2(1, 0.54f);
            engageLabel.rectTransform.offsetMin = new Vector2(6f, 0f);
            engageLabel.rectTransform.offsetMax = new Vector2(-14f, -6f);

            // Button area
            // Ignore button (left)
            Button ignoreBtn = HubUIHelper.CreateButton(cardRect, "Ignore",
                HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.08f), HubUIHelper.SubTextColor, 14);
            RectTransform ignoreBtnRect = ignoreBtn.GetComponent<RectTransform>();
            ignoreBtnRect.anchorMin = new Vector2(0.05f, 0.04f);
            ignoreBtnRect.anchorMax = new Vector2(0.47f, 0.20f);
            ignoreBtnRect.offsetMin = Vector2.zero;
            ignoreBtnRect.offsetMax = Vector2.zero;

            // Engage button (right)
            Button engageBtn = HubUIHelper.CreateButton(cardRect, "Engage",
                HubUIHelper.AccentColor, Color.black, 14);
            RectTransform engageBtnRect = engageBtn.GetComponent<RectTransform>();
            engageBtnRect.anchorMin = new Vector2(0.53f, 0.04f);
            engageBtnRect.anchorMax = new Vector2(0.95f, 0.20f);
            engageBtnRect.offsetMin = Vector2.zero;
            engageBtnRect.offsetMax = Vector2.zero;

            // Wire button callbacks in play mode
            if (Application.isPlaying && HubManager.Instance != null)
            {
                var day = HubManager.Instance.CurrentDay;
                if (day != null)
                {
                    DecisionCardSO cardSO = null;
                    foreach (var c in day.availableCards)
                    {
                        if (c.cardTitle == title) { cardSO = c; break; }
                    }

                    if (cardSO != null)
                    {
                        DecisionCardSO capturedCard = cardSO;
                        ignoreBtn.onClick.AddListener(() =>
                        {
                            HubManager.Instance.MakeDecision(capturedCard, false);
                            cardGO.SetActive(false);
                        });
                        engageBtn.onClick.AddListener(() =>
                        {
                            HubManager.Instance.MakeDecision(capturedCard, true);
                            cardGO.SetActive(false);
                        });
                    }
                }
            }
        }
    }
}

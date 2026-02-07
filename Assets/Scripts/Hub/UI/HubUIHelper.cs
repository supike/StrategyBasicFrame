using UnityEngine;
using UnityEngine.UI;

namespace Hub.UI
{
    public static class HubUIHelper
    {
        // Color palette - "Always lacking, always dangerous"
        public static readonly Color BackgroundColor = new Color(0.04f, 0.05f, 0.08f, 0.96f);
        public static readonly Color PanelColor = new Color(0.10f, 0.12f, 0.18f, 0.95f);
        public static readonly Color AccentColor = new Color(0.32f, 0.67f, 0.91f, 1f);
        public static readonly Color DangerColor = new Color(0.85f, 0.2f, 0.15f, 1f);
        public static readonly Color WarningColor = new Color(0.9f, 0.65f, 0.1f, 1f);
        public static readonly Color TensionColor = new Color(0.6f, 0.1f, 0.15f, 0.8f);
        public static readonly Color CardColor = new Color(0.12f, 0.14f, 0.20f, 0.98f);
        public static readonly Color ConfirmButtonColor = new Color(0.7f, 0.15f, 0.1f, 1f);
        public static readonly Color DisabledTextColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        public static readonly Color TextColor = new Color(0.85f, 0.85f, 0.9f, 1f);
        public static readonly Color SubTextColor = new Color(0.6f, 0.6f, 0.65f, 1f);

        private static Font cachedFont;
        public static Font DefaultFont
        {
            get
            {
                if (cachedFont == null)
                    cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return cachedFont;
            }
        }

        public static RectTransform CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            Image img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;

            return rect;
        }

        public static Text CreateText(Transform parent, string content, int fontSize,
            TextAnchor anchor, Color color)
        {
            GameObject go = new GameObject("Text_" + content.Replace(" ", ""), typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);

            Text text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.font = DefaultFont;
            text.raycastTarget = false;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return text;
        }

        public static Button CreateButton(Transform parent, string label, Color bgColor, Color textColor, int fontSize = 20)
        {
            GameObject go = new GameObject(label + "Btn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            Image img = go.GetComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            Button btn = go.GetComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = Lighten(bgColor, 0.15f);
            colors.pressedColor = Lighten(bgColor, -0.15f);
            colors.selectedColor = bgColor;
            colors.disabledColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.4f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;

            Text labelText = CreateText(go.transform, label.ToUpperInvariant(), fontSize, TextAnchor.MiddleCenter, textColor);
            labelText.raycastTarget = false;

            return btn;
        }

        public static RectTransform CreateScrollView(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            // ScrollView root
            RectTransform scrollRect = CreatePanel(parent, name, anchorMin, anchorMax, offsetMin, offsetMax, Color.clear);
            ScrollRect scroll = scrollRect.gameObject.AddComponent<ScrollRect>();
            scrollRect.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            scrollRect.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f); // needed for mask
            scrollRect.GetComponent<Image>().raycastTarget = true;

            // Content
            GameObject contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(scrollRect, false);

            RectTransform content = contentGO.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup layout = contentGO.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(8, 8, 8, 8);

            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            return content;
        }

        public static Image CreateProgressBar(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax,
            Color bgColor, Color fillColor, float fillAmount = 0.5f)
        {
            RectTransform bg = CreatePanel(parent, name + "BG", anchorMin, anchorMax, offsetMin, offsetMax, bgColor);

            GameObject fillGO = new GameObject(name + "Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGO.transform.SetParent(bg, false);

            Image fill = fillGO.GetComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = fillAmount;
            fill.raycastTarget = false;

            RectTransform fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            return fill;
        }

        public static Color Lighten(Color color, float amount)
        {
            if (amount >= 0f)
                return Color.Lerp(color, Color.white, amount);
            return Color.Lerp(color, Color.black, Mathf.Abs(amount));
        }

        public static Color GetEmotionColor(EmotionCondition condition)
        {
            return condition switch
            {
                EmotionCondition.desperatated => DangerColor,
                EmotionCondition.bad => WarningColor,
                EmotionCondition.normal => SubTextColor,
                EmotionCondition.good => AccentColor,
                EmotionCondition.happy => new Color(0.2f, 0.8f, 0.4f, 1f),
                _ => SubTextColor
            };
        }

        public static Color GetDangerSignalColor(DangerSignal signal)
        {
            return signal switch
            {
                DangerSignal.CollapseImminent => DangerColor,
                DangerSignal.Trauma => WarningColor,
                DangerSignal.Rebellious => new Color(0.9f, 0.5f, 0.1f, 1f),
                _ => Color.clear
            };
        }

        public static Color GetCardTypeColor(DecisionCardType type)
        {
            return type switch
            {
                DecisionCardType.ConflictIntervention => DangerColor,
                DecisionCardType.InternalIssue => WarningColor,
                DecisionCardType.ExternalProposal => AccentColor,
                DecisionCardType.IntelReport => new Color(0.5f, 0.4f, 0.8f, 1f),
                _ => SubTextColor
            };
        }

        public static string GetCardTypeLabel(DecisionCardType type)
        {
            return type switch
            {
                DecisionCardType.ConflictIntervention => "CONFLICT",
                DecisionCardType.InternalIssue => "INTERNAL",
                DecisionCardType.ExternalProposal => "PROPOSAL",
                DecisionCardType.IntelReport => "INTEL",
                _ => "UNKNOWN"
            };
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Hub.UI
{
    /// <summary>
    /// Builds the top fixed bar: Day counter | Organization Stability | Resources | World Tension
    /// </summary>
    public class HubTopBarBuilder
    {
        private Text dayText;
        private Text stabilityText;
        private Image stabilityFill;
        private Text personnelText;
        private Text informationText;
        private Text influenceText;
        private Image tensionFill;

        public void Build(RectTransform parent)
        {
            // Day counter (left 15%)
            RectTransform dayBlock = HubUIHelper.CreatePanel(parent, "DayBlock",
                new Vector2(0f, 0f), new Vector2(0.15f, 1f),
                new Vector2(12f, 6f), new Vector2(-4f, -6f),
                HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.05f));

            Text dayLabel = HubUIHelper.CreateText(dayBlock, "DAY", 12, TextAnchor.UpperLeft, HubUIHelper.SubTextColor);
            dayLabel.rectTransform.anchorMin = new Vector2(0, 0);
            dayLabel.rectTransform.anchorMax = new Vector2(1, 1);
            dayLabel.rectTransform.offsetMin = new Vector2(10f, 0f);
            dayLabel.rectTransform.offsetMax = new Vector2(-10f, -4f);

            dayText = HubUIHelper.CreateText(dayBlock, "1", 32, TextAnchor.MiddleCenter, Color.white);
            dayText.fontStyle = FontStyle.Bold;

            // Organization Stability (15-35%)
            RectTransform stabilityBlock = HubUIHelper.CreatePanel(parent, "StabilityBlock",
                new Vector2(0.15f, 0f), new Vector2(0.35f, 1f),
                new Vector2(4f, 6f), new Vector2(-4f, -6f),
                HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.03f));

            Text stabLabel = HubUIHelper.CreateText(stabilityBlock, "STABILITY", 11, TextAnchor.UpperLeft, HubUIHelper.SubTextColor);
            stabLabel.rectTransform.anchorMin = new Vector2(0, 0.6f);
            stabLabel.rectTransform.anchorMax = new Vector2(1, 1);
            stabLabel.rectTransform.offsetMin = new Vector2(10f, 0f);
            stabLabel.rectTransform.offsetMax = new Vector2(-10f, -4f);

            stabilityText = HubUIHelper.CreateText(stabilityBlock, "Uneasy", 14, TextAnchor.MiddleLeft, HubUIHelper.WarningColor);
            stabilityText.rectTransform.anchorMin = new Vector2(0, 0.3f);
            stabilityText.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
            stabilityText.rectTransform.offsetMin = new Vector2(10f, 0f);
            stabilityText.rectTransform.offsetMax = new Vector2(-4f, 0f);

            // Stability bar
            stabilityFill = HubUIHelper.CreateProgressBar(stabilityBlock, "StabilityBar",
                new Vector2(0f, 0.05f), new Vector2(1f, 0.3f),
                new Vector2(10f, 2f), new Vector2(-10f, -2f),
                new Color(0.06f, 0.07f, 0.1f, 1f), HubUIHelper.WarningColor, 0.5f);

            // Resources: Personnel (35-50%), Information (50-65%), Influence (65-80%)
            BuildResourceBlock(parent, "PersonnelBlock", "PERSONNEL", "12",
                new Vector2(0.35f, 0f), new Vector2(0.50f, 1f), out personnelText);
            BuildResourceBlock(parent, "InformationBlock", "INFORMATION", "5",
                new Vector2(0.50f, 0f), new Vector2(0.65f, 1f), out informationText);
            BuildResourceBlock(parent, "InfluenceBlock", "INFLUENCE", "8",
                new Vector2(0.65f, 0f), new Vector2(0.80f, 1f), out influenceText);

            // World Tension (80-100%)
            RectTransform tensionBlock = HubUIHelper.CreatePanel(parent, "TensionBlock",
                new Vector2(0.80f, 0f), new Vector2(1f, 1f),
                new Vector2(4f, 6f), new Vector2(-12f, -6f),
                HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.02f));

            Text tensionLabel = HubUIHelper.CreateText(tensionBlock, "WORLD TENSION", 10, TextAnchor.UpperLeft, HubUIHelper.SubTextColor);
            tensionLabel.rectTransform.anchorMin = new Vector2(0, 0.6f);
            tensionLabel.rectTransform.anchorMax = new Vector2(1, 1);
            tensionLabel.rectTransform.offsetMin = new Vector2(10f, 0f);
            tensionLabel.rectTransform.offsetMax = new Vector2(-10f, -4f);

            tensionFill = HubUIHelper.CreateProgressBar(tensionBlock, "TensionBar",
                new Vector2(0f, 0.1f), new Vector2(1f, 0.55f),
                new Vector2(10f, 2f), new Vector2(-10f, -2f),
                new Color(0.06f, 0.07f, 0.1f, 1f), HubUIHelper.TensionColor, 0.1f);
        }

        private void BuildResourceBlock(RectTransform parent, string name, string label, string defaultValue,
            Vector2 anchorMin, Vector2 anchorMax, out Text valueText)
        {
            RectTransform block = HubUIHelper.CreatePanel(parent, name,
                anchorMin, anchorMax,
                new Vector2(4f, 6f), new Vector2(-4f, -6f),
                HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.04f));

            Text labelText = HubUIHelper.CreateText(block, label, 10, TextAnchor.UpperLeft, HubUIHelper.SubTextColor);
            labelText.rectTransform.anchorMin = new Vector2(0, 0.55f);
            labelText.rectTransform.anchorMax = new Vector2(1, 1);
            labelText.rectTransform.offsetMin = new Vector2(10f, 0f);
            labelText.rectTransform.offsetMax = new Vector2(-10f, -4f);

            valueText = HubUIHelper.CreateText(block, defaultValue, 26, TextAnchor.MiddleCenter, Color.white);
            valueText.fontStyle = FontStyle.Bold;
            valueText.rectTransform.anchorMin = new Vector2(0, 0);
            valueText.rectTransform.anchorMax = new Vector2(1, 0.6f);
            valueText.rectTransform.offsetMin = new Vector2(4f, 0f);
            valueText.rectTransform.offsetMax = new Vector2(-4f, 0f);
        }

        public void Refresh()
        {
            if (HubManager.Instance != null)
            {
                dayText.text = HubManager.Instance.GetDayNumber().ToString();

                var org = HubManager.Instance.Organization;
                stabilityText.text = org.GetStabilityLabel();
                stabilityFill.fillAmount = org.stability / 100f;

                // Color stability bar based on value
                if (org.stability >= 60f)
                    stabilityFill.color = new Color(0.2f, 0.6f, 0.3f, 1f);
                else if (org.stability >= 30f)
                    stabilityFill.color = HubUIHelper.WarningColor;
                else
                    stabilityFill.color = HubUIHelper.DangerColor;
            }

            if (HubResourceManager.Instance != null)
            {
                personnelText.text = HubResourceManager.Instance.GetResource(HubResourceType.Personnel).ToString();
                informationText.text = HubResourceManager.Instance.GetResource(HubResourceType.Information).ToString();
                influenceText.text = HubResourceManager.Instance.GetResource(HubResourceType.Influence).ToString();

                tensionFill.fillAmount = HubResourceManager.Instance.WorldTension / 100f;
                // Tension bar gets redder as it increases
                float t = HubResourceManager.Instance.WorldTension / 100f;
                tensionFill.color = Color.Lerp(HubUIHelper.WarningColor, HubUIHelper.DangerColor, t);
            }
        }
    }
}

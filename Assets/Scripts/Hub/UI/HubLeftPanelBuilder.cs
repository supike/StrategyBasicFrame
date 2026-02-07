using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BattleYouCan;
using Core;

namespace Hub.UI
{
    /// <summary>
    /// Builds the left management panel with 4 tabs: Units, Organization, Relations, Records
    /// </summary>
    public class HubLeftPanelBuilder
    {
        private RectTransform unitContent;
        private RectTransform orgContent;
        private RectTransform relationsContent;
        private RectTransform recordsContent;

        public void Build(RectTransform parent, Canvas canvas)
        {
            // Tab button strip (top portion of left panel)
            RectTransform tabStrip = HubUIHelper.CreatePanel(parent, "TabStrip",
                new Vector2(0f, 0.93f), new Vector2(1f, 1f),
                new Vector2(4f, -2f), new Vector2(-4f, -2f),
                HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.06f));

            // Content area
            RectTransform contentArea = HubUIHelper.CreatePanel(parent, "TabContentArea",
                new Vector2(0f, 0f), new Vector2(1f, 0.93f),
                new Vector2(2f, 2f), new Vector2(-2f, -2f),
                Color.clear);

            // Build tab contents
            unitContent = BuildUnitTabContent(contentArea);
            orgContent = BuildOrgTabContent(contentArea);
            relationsContent = BuildRelationsTabContent(contentArea);
            recordsContent = BuildRecordsTabContent(contentArea);

            // Build tab buttons and wire to HubUIManager
            string[] tabLabels = { "UNIT", "ORG", "REL", "REC" };
            GameObject[] contents = { unitContent.gameObject, orgContent.gameObject, relationsContent.gameObject, recordsContent.gameObject };

            HubUIManager manager = canvas.GetComponent<HubUIManager>();
            if (manager == null)
                manager = canvas.gameObject.AddComponent<HubUIManager>();

            var tabButtons = new List<HubUIManager.TabButton>();

            for (int i = 0; i < tabLabels.Length; i++)
            {
                float min = i * 0.25f;
                float max = (i + 1) * 0.25f;

                RectTransform tabBtnRect = HubUIHelper.CreatePanel(tabStrip, "Tab_" + tabLabels[i],
                    new Vector2(min, 0), new Vector2(max, 1),
                    new Vector2(1f, 1f), new Vector2(-1f, -1f),
                    HubUIHelper.PanelColor);
                tabBtnRect.GetComponent<Image>().raycastTarget = true;

                Button btn = tabBtnRect.gameObject.AddComponent<Button>();
                ColorBlock colors = btn.colors;
                colors.normalColor = HubUIHelper.PanelColor;
                colors.highlightedColor = HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.1f);
                colors.pressedColor = HubUIHelper.Lighten(HubUIHelper.PanelColor, -0.1f);
                colors.selectedColor = HubUIHelper.AccentColor;
                colors.colorMultiplier = 1f;
                btn.colors = colors;

                Text label = HubUIHelper.CreateText(tabBtnRect, tabLabels[i], 12, TextAnchor.MiddleCenter, HubUIHelper.TextColor);
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = Vector2.zero;

                contents[i].SetActive(i == 0);

                tabButtons.Add(new HubUIManager.TabButton
                {
                    button = btn,
                    content = contents[i],
                    activeColor = HubUIHelper.AccentColor,
                    inactiveColor = HubUIHelper.PanelColor
                });
            }

            manager.ConfigureTabs(tabButtons.ToArray(), 0);
        }

        private RectTransform BuildUnitTabContent(RectTransform parent)
        {
            RectTransform panel = HubUIHelper.CreatePanel(parent, "UnitTabContent",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                HubUIHelper.Lighten(HubUIHelper.PanelColor, -0.02f));

            // Title
            Text title = HubUIHelper.CreateText(panel, "UNIT STATUS", 14, TextAnchor.UpperLeft, HubUIHelper.TextColor);
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.anchorMin = new Vector2(0, 0.93f);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.offsetMin = new Vector2(10f, 0f);
            title.rectTransform.offsetMax = new Vector2(-10f, -4f);

            // Scrollable unit list
            RectTransform scrollContent = HubUIHelper.CreateScrollView(panel, "UnitList",
                new Vector2(0, 0), new Vector2(1, 0.93f),
                new Vector2(4f, 4f), new Vector2(-4f, -2f));

            // Populate with placeholder units or real data
            if (Application.isPlaying && GameManager.Instance != null)
            {
                var units = GameManager.Instance.UnitManager.GetAlivePlayerUnits();
                foreach (var unit in units)
                {
                    BuildUnitRow(scrollContent, new UnitHubStatus(unit));
                }
            }
            else
            {
                // Editor placeholder data
                string[] placeholderUnits = { "Commander Rex", "Medic Yuna", "Scout Jin", "Engineer Park", "Sniper Hana" };
                string[] placeholderEmotions = { "Stable", "Troubled", "Content", "Desperate", "High Spirits" };
                string[] placeholderDangers = { "", "", "", "COLLAPSE IMMINENT", "" };

                for (int i = 0; i < placeholderUnits.Length; i++)
                {
                    BuildPlaceholderUnitRow(scrollContent, placeholderUnits[i], placeholderEmotions[i], placeholderDangers[i]);
                }
            }

            return panel;
        }

        private void BuildUnitRow(RectTransform parent, UnitHubStatus status)
        {
            string unitName = status.unit.unitData != null ? status.unit.unitData.characterName : "Unknown";
            string emotion = status.GetEmotionSummary();
            string danger = status.GetDangerSignalText();

            BuildPlaceholderUnitRow(parent, unitName, emotion, danger);
        }

        private void BuildPlaceholderUnitRow(RectTransform parent, string unitName, string emotion, string danger)
        {
            GameObject rowGO = new GameObject("UnitRow_" + unitName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rowGO.transform.SetParent(parent, false);

            Image rowBg = rowGO.GetComponent<Image>();
            rowBg.color = HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.06f);
            rowBg.raycastTarget = true;

            RectTransform rowRect = rowGO.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 52f);

            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.minHeight = 52f;
            le.preferredHeight = 52f;

            // Unit name
            Text nameText = HubUIHelper.CreateText(rowGO.transform, unitName, 13, TextAnchor.MiddleLeft, Color.white);
            nameText.rectTransform.anchorMin = new Vector2(0, 0.5f);
            nameText.rectTransform.anchorMax = new Vector2(0.95f, 1f);
            nameText.rectTransform.offsetMin = new Vector2(8f, 0f);
            nameText.rectTransform.offsetMax = new Vector2(-8f, -2f);

            // Emotion text
            Color emotionColor = HubUIHelper.SubTextColor;
            if (emotion == "Desperate") emotionColor = HubUIHelper.DangerColor;
            else if (emotion == "Troubled") emotionColor = HubUIHelper.WarningColor;
            else if (emotion == "Content" || emotion == "High Spirits") emotionColor = HubUIHelper.AccentColor;

            Text emotionText = HubUIHelper.CreateText(rowGO.transform, emotion, 11, TextAnchor.MiddleLeft, emotionColor);
            emotionText.rectTransform.anchorMin = new Vector2(0, 0);
            emotionText.rectTransform.anchorMax = new Vector2(0.6f, 0.5f);
            emotionText.rectTransform.offsetMin = new Vector2(8f, 2f);
            emotionText.rectTransform.offsetMax = new Vector2(-4f, 0f);

            // Danger signal
            if (!string.IsNullOrEmpty(danger))
            {
                Text dangerText = HubUIHelper.CreateText(rowGO.transform, danger, 9, TextAnchor.MiddleRight, HubUIHelper.DangerColor);
                dangerText.fontStyle = FontStyle.Bold;
                dangerText.rectTransform.anchorMin = new Vector2(0.4f, 0);
                dangerText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                dangerText.rectTransform.offsetMin = new Vector2(4f, 2f);
                dangerText.rectTransform.offsetMax = new Vector2(-8f, 0f);
            }
        }

        private RectTransform BuildOrgTabContent(RectTransform parent)
        {
            RectTransform panel = HubUIHelper.CreatePanel(parent, "OrgTabContent",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                HubUIHelper.Lighten(HubUIHelper.PanelColor, -0.02f));

            Text title = HubUIHelper.CreateText(panel, "ORGANIZATION", 14, TextAnchor.UpperLeft, HubUIHelper.TextColor);
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.anchorMin = new Vector2(0, 0.93f);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.offsetMin = new Vector2(10f, 0f);
            title.rectTransform.offsetMax = new Vector2(-10f, -4f);

            RectTransform scrollContent = HubUIHelper.CreateScrollView(panel, "OrgList",
                new Vector2(0, 0), new Vector2(1, 0.93f),
                new Vector2(4f, 4f), new Vector2(-4f, -2f));

            // Stability bar
            BuildOrgStatRow(scrollContent, "Stability", "Uneasy", 0.5f, HubUIHelper.WarningColor);
            BuildOrgStatRow(scrollContent, "Cruelty Index", "Low", 0.1f, HubUIHelper.DangerColor);
            BuildOrgStatRow(scrollContent, "Trust Level", "Moderate", 0.5f, HubUIHelper.AccentColor);

            // Separator
            GameObject sepGO = new GameObject("Separator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sepGO.transform.SetParent(scrollContent, false);
            sepGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
            sepGO.GetComponent<Image>().raycastTarget = false;
            LayoutElement sepLE = sepGO.AddComponent<LayoutElement>();
            sepLE.minHeight = 2f;
            sepLE.preferredHeight = 2f;

            // Long-term effects header
            GameObject effectHeader = new GameObject("EffectHeader", typeof(RectTransform));
            effectHeader.transform.SetParent(scrollContent, false);
            Text headerText = HubUIHelper.CreateText(effectHeader.transform, "LONG-TERM EFFECTS", 11, TextAnchor.MiddleLeft, HubUIHelper.SubTextColor);
            headerText.fontStyle = FontStyle.Bold;
            LayoutElement headerLE = effectHeader.AddComponent<LayoutElement>();
            headerLE.minHeight = 28f;
            headerLE.preferredHeight = 28f;

            // Effect entries
            string[] effects = { "No significant long-term effects." };
            foreach (string effect in effects)
            {
                GameObject effectGO = new GameObject("Effect", typeof(RectTransform));
                effectGO.transform.SetParent(scrollContent, false);
                Text effectText = HubUIHelper.CreateText(effectGO.transform, "  " + effect, 11, TextAnchor.MiddleLeft, HubUIHelper.SubTextColor);
                LayoutElement effectLE = effectGO.AddComponent<LayoutElement>();
                effectLE.minHeight = 24f;
                effectLE.preferredHeight = 24f;
            }

            return panel;
        }

        private void BuildOrgStatRow(RectTransform parent, string label, string valueLabel, float fillAmount, Color barColor)
        {
            GameObject rowGO = new GameObject("OrgStat_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rowGO.transform.SetParent(parent, false);
            rowGO.GetComponent<Image>().color = HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.04f);
            rowGO.GetComponent<Image>().raycastTarget = false;

            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.minHeight = 56f;
            le.preferredHeight = 56f;

            // Label
            Text labelText = HubUIHelper.CreateText(rowGO.transform, label.ToUpperInvariant(), 10, TextAnchor.UpperLeft, HubUIHelper.SubTextColor);
            labelText.rectTransform.anchorMin = new Vector2(0, 0.55f);
            labelText.rectTransform.anchorMax = new Vector2(0.6f, 1);
            labelText.rectTransform.offsetMin = new Vector2(8f, 0f);
            labelText.rectTransform.offsetMax = new Vector2(-4f, -4f);

            // Value
            Text valueText = HubUIHelper.CreateText(rowGO.transform, valueLabel, 12, TextAnchor.UpperRight, barColor);
            valueText.rectTransform.anchorMin = new Vector2(0.6f, 0.55f);
            valueText.rectTransform.anchorMax = new Vector2(1, 1);
            valueText.rectTransform.offsetMin = new Vector2(4f, 0f);
            valueText.rectTransform.offsetMax = new Vector2(-8f, -4f);

            // Progress bar
            HubUIHelper.CreateProgressBar(rowGO.transform, label + "Bar",
                new Vector2(0, 0.1f), new Vector2(1, 0.45f),
                new Vector2(8f, 0f), new Vector2(-8f, 0f),
                new Color(0.06f, 0.07f, 0.1f, 1f), barColor, fillAmount);
        }

        private RectTransform BuildRelationsTabContent(RectTransform parent)
        {
            RectTransform panel = HubUIHelper.CreatePanel(parent, "RelationsTabContent",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                HubUIHelper.Lighten(HubUIHelper.PanelColor, -0.02f));

            Text title = HubUIHelper.CreateText(panel, "RELATIONS", 14, TextAnchor.UpperLeft, HubUIHelper.TextColor);
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.anchorMin = new Vector2(0, 0.93f);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.offsetMin = new Vector2(10f, 0f);
            title.rectTransform.offsetMax = new Vector2(-10f, -4f);

            RectTransform scrollContent = HubUIHelper.CreateScrollView(panel, "RelationsList",
                new Vector2(0, 0), new Vector2(1, 0.93f),
                new Vector2(4f, 4f), new Vector2(-4f, -2f));

            // Placeholder entries
            BuildRelationRow(scrollContent, "Rex <-> Yuna", "Synergy", HubUIHelper.AccentColor);
            BuildRelationRow(scrollContent, "Jin <-> Park", "Tension", HubUIHelper.WarningColor);
            BuildRelationRow(scrollContent, "Internal Conflict", "Low probability", HubUIHelper.SubTextColor);

            return panel;
        }

        private void BuildRelationRow(RectTransform parent, string pair, string status, Color statusColor)
        {
            GameObject rowGO = new GameObject("Relation_" + pair, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rowGO.transform.SetParent(parent, false);
            rowGO.GetComponent<Image>().color = HubUIHelper.Lighten(HubUIHelper.PanelColor, 0.04f);
            rowGO.GetComponent<Image>().raycastTarget = false;

            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.minHeight = 40f;
            le.preferredHeight = 40f;

            Text pairText = HubUIHelper.CreateText(rowGO.transform, pair, 12, TextAnchor.MiddleLeft, HubUIHelper.TextColor);
            pairText.rectTransform.anchorMin = new Vector2(0, 0);
            pairText.rectTransform.anchorMax = new Vector2(0.55f, 1);
            pairText.rectTransform.offsetMin = new Vector2(8f, 0f);
            pairText.rectTransform.offsetMax = new Vector2(-4f, 0f);

            Text statusText = HubUIHelper.CreateText(rowGO.transform, status, 11, TextAnchor.MiddleRight, statusColor);
            statusText.rectTransform.anchorMin = new Vector2(0.55f, 0);
            statusText.rectTransform.anchorMax = new Vector2(1, 1);
            statusText.rectTransform.offsetMin = new Vector2(4f, 0f);
            statusText.rectTransform.offsetMax = new Vector2(-8f, 0f);
        }

        private RectTransform BuildRecordsTabContent(RectTransform parent)
        {
            RectTransform panel = HubUIHelper.CreatePanel(parent, "RecordsTabContent",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                HubUIHelper.Lighten(HubUIHelper.PanelColor, -0.02f));

            Text title = HubUIHelper.CreateText(panel, "RECORDS", 14, TextAnchor.UpperLeft, HubUIHelper.TextColor);
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.anchorMin = new Vector2(0, 0.93f);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.offsetMin = new Vector2(10f, 0f);
            title.rectTransform.offsetMax = new Vector2(-10f, -4f);

            RectTransform scrollContent = HubUIHelper.CreateScrollView(panel, "RecordsList",
                new Vector2(0, 0), new Vector2(1, 0.93f),
                new Vector2(4f, 4f), new Vector2(-4f, -2f));

            // Section: Recent Combat
            BuildRecordEntry(scrollContent, "RECENT COMBAT", "", HubUIHelper.SubTextColor, true);
            BuildRecordEntry(scrollContent, "  Skirmish at Sector 7", "Day 1 - Victory", HubUIHelper.AccentColor, false);
            BuildRecordEntry(scrollContent, "  Ambush at Bridge", "Day 1 - Pyrrhic", HubUIHelper.WarningColor, false);

            // Section: Casualties
            BuildRecordEntry(scrollContent, "CASUALTIES", "", HubUIHelper.SubTextColor, true);
            BuildRecordEntry(scrollContent, "  No casualties yet", "", HubUIHelper.SubTextColor, false);

            // Section: World Changes
            BuildRecordEntry(scrollContent, "WORLD CHANGES", "", HubUIHelper.SubTextColor, true);
            BuildRecordEntry(scrollContent, "  Tension rising in the east", "", HubUIHelper.WarningColor, false);

            return panel;
        }

        private void BuildRecordEntry(RectTransform parent, string text, string detail, Color textColor, bool isHeader)
        {
            GameObject rowGO = new GameObject("Record", typeof(RectTransform));
            rowGO.transform.SetParent(parent, false);

            LayoutElement le = rowGO.AddComponent<LayoutElement>();
            le.minHeight = isHeader ? 30f : 26f;
            le.preferredHeight = isHeader ? 30f : 26f;

            int fontSize = isHeader ? 11 : 11;
            FontStyle style = isHeader ? FontStyle.Bold : FontStyle.Normal;

            Text entryText = HubUIHelper.CreateText(rowGO.transform, text, fontSize, TextAnchor.MiddleLeft, textColor);
            entryText.fontStyle = style;
            entryText.rectTransform.anchorMin = new Vector2(0, 0);
            entryText.rectTransform.anchorMax = new Vector2(0.6f, 1);
            entryText.rectTransform.offsetMin = new Vector2(4f, 0f);
            entryText.rectTransform.offsetMax = new Vector2(-4f, 0f);

            if (!string.IsNullOrEmpty(detail))
            {
                Text detailText = HubUIHelper.CreateText(rowGO.transform, detail, 10, TextAnchor.MiddleRight, HubUIHelper.Lighten(textColor, -0.2f));
                detailText.rectTransform.anchorMin = new Vector2(0.6f, 0);
                detailText.rectTransform.anchorMax = new Vector2(1, 1);
                detailText.rectTransform.offsetMin = new Vector2(4f, 0f);
                detailText.rectTransform.offsetMax = new Vector2(-4f, 0f);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BattleYouCan
{
    /// <summary>
    /// Lightweight 2D tactical grid UI builder. Generates a squad deploy preview with tile grid and unit slots.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class HubUI2DBootstrap : MonoBehaviour
    {
        [SerializeField] private Color backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
        [SerializeField] private Color gridColor = new Color(0.25f, 0.28f, 0.35f, 0.6f);
        [SerializeField] private Color validTileColor = new Color(0.2f, 0.5f, 0.3f, 0.4f);
        [SerializeField] private Color unitSlotColor = new Color(0.32f, 0.67f, 0.91f, 0.8f);
        [SerializeField] private Color accentColor = new Color(0.32f, 0.67f, 0.91f, 1f);

        private bool initialized;
        private const int GridWidth = 8;
        private const int GridHeight = 6;
        private const float TileSize = 60f;

        private void OnEnable()
        {
            initialized = false;
            BuildIfNeeded();
        }

        private void Start()
        {
            BuildIfNeeded();
        }

        private void BuildIfNeeded()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            Canvas uiCanvas = EnsureUICanvas();
            BuildGameplayUI(uiCanvas.transform);

            CreateGridDisplay();
            CreateUnitSlots();
        }

        private Canvas EnsureUICanvas()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas candidate in canvases)
            {
                if (candidate.isRootCanvas && candidate.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return candidate;
                }
            }

            GameObject canvasGO = new GameObject("GameUI2DCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.layer = LayerMask.NameToLayer("UI");

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            return canvas;
        }

        private void BuildGameplayUI(Transform canvasTransform)
        {
            if (canvasTransform.Find("GameplayPanel") != null)
            {
                return;
            }

            // Background panel
            RectTransform bg = CreatePanel(canvasTransform, "GameplayPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, backgroundColor);

            // Top info bar
            CreateInfoBar(bg);

            // Bottom action bar
            CreateActionBar(bg);
        }

        private void CreateInfoBar(RectTransform parent)
        {
            RectTransform bar = CreatePanel(parent, "InfoBar", new Vector2(0f, 0.9f), new Vector2(1f, 1f), new Vector2(20f, -20f), new Vector2(-20f, -20f), Darken(backgroundColor, 0.15f));

            Text missionText = CreateText(bar, "Mission: Secure Facility Omega", 28, TextAnchor.MiddleLeft, Color.white);
            RectTransform missionRect = missionText.rectTransform;
            missionRect.anchorMin = new Vector2(0f, 0f);
            missionRect.anchorMax = new Vector2(0.6f, 1f);
            missionRect.offsetMin = new Vector2(30f, 0f);
            missionRect.offsetMax = new Vector2(-30f, 0f);

            Text turnText = CreateText(bar, "Turn 1 / 12", 24, TextAnchor.MiddleRight, accentColor);
            RectTransform turnRect = turnText.rectTransform;
            turnRect.anchorMin = new Vector2(0.7f, 0f);
            turnRect.anchorMax = new Vector2(1f, 1f);
            turnRect.offsetMin = new Vector2(10f, 0f);
            turnRect.offsetMax = new Vector2(-30f, 0f);
        }

        private void CreateActionBar(RectTransform parent)
        {
            RectTransform bar = CreatePanel(parent, "ActionBar", new Vector2(0f, 0f), new Vector2(1f, 0.1f), new Vector2(20f, 20f), new Vector2(-20f, 20f), Darken(backgroundColor, 0.2f));

            Button endTurnBtn = CreateButton(bar, "End Turn", 0.1f);
            Button settingsBtn = CreateButton(bar, "Settings", 0.5f);
            Button retreatBtn = CreateButton(bar, "Retreat", 0.9f);
        }

        private void CreateGridDisplay()
        {
            GameObject gridGO = new GameObject("TacticalGrid", typeof(RectTransform));
            gridGO.layer = LayerMask.NameToLayer("UI");

            RectTransform gridRect = gridGO.GetComponent<RectTransform>();
            gridRect.anchoredPosition = new Vector2(150f, -250f);
            gridRect.sizeDelta = new Vector2(GridWidth * TileSize, GridHeight * TileSize);

            Canvas gridCanvas = gridGO.AddComponent<Canvas>();
            gridCanvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = gridGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GraphicRaycaster raycaster = gridGO.AddComponent<GraphicRaycaster>();

            for (int y = 0; y < GridHeight; y++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    CreateTile(gridRect, x, y);
                }
            }
        }

        private void CreateTile(RectTransform parent, int x, int y)
        {
            GameObject tileGO = new GameObject($"Tile_{x}_{y}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tileGO.transform.SetParent(parent, false);

            RectTransform rect = tileGO.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x * TileSize + TileSize * 0.5f, -y * TileSize - TileSize * 0.5f);
            rect.sizeDelta = new Vector2(TileSize - 2f, TileSize - 2f);

            Image image = tileGO.GetComponent<Image>();
            image.color = gridColor;
            image.raycastTarget = true;

            // Add border outline
            Image border = new GameObject("Border").AddComponent<Image>();
            border.transform.SetParent(tileGO.transform, false);
            border.rectTransform.anchoredPosition = Vector2.zero;
            border.rectTransform.sizeDelta = Vector2.zero;
            border.color = new Color(1f, 1f, 1f, 0.2f);
            border.raycastTarget = false;
        }

        private void CreateUnitSlots()
        {
            GameObject slotsGO = new GameObject("UnitDeploySlots", typeof(RectTransform));
            slotsGO.layer = LayerMask.NameToLayer("UI");

            RectTransform slotsRect = slotsGO.GetComponent<RectTransform>();
            slotsRect.anchoredPosition = new Vector2(1000f, -350f);
            slotsRect.sizeDelta = new Vector2(300f, 400f);

            Canvas slotsCanvas = slotsGO.AddComponent<Canvas>();
            slotsCanvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = slotsGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // Title
            Text title = CreateText(slotsRect, "SQUAD ROSTER", 24, TextAnchor.UpperCenter, Color.white);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchoredPosition = new Vector2(0f, -25f);
            titleRect.sizeDelta = new Vector2(280f, 40f);

            // Unit slots
            string[] units = { "Vanguard Alpha", "Medic Echo", "Sniper Lynx", "Engineer Bolt" };
            for (int i = 0; i < units.Length; i++)
            {
                CreateUnitSlot(slotsRect, units[i], i);
            }
        }

        private void CreateUnitSlot(RectTransform parent, string unitName, int index)
        {
            RectTransform slotBg = CreatePanel(parent, $"Slot_{index}", new Vector2(0f, 1f), new Vector2(1f, 1f), 
                new Vector2(10f, -80f - (index * 75f)), new Vector2(-10f, -130f - (index * 75f)), 
                Lighten(unitSlotColor, -0.3f));

            Text unitText = CreateText(slotBg, unitName, 18, TextAnchor.MiddleCenter, Color.white);
            unitText.rectTransform.offsetMin = Vector2.zero;
            unitText.rectTransform.offsetMax = Vector2.zero;

            Button selectBtn = slotBg.gameObject.AddComponent<Button>();
            selectBtn.targetGraphic = slotBg.GetComponent<Image>();
            
            ColorBlock colors = selectBtn.colors;
            colors.normalColor = Lighten(unitSlotColor, -0.3f);
            colors.highlightedColor = Lighten(unitSlotColor, -0.1f);
            colors.pressedColor = Lighten(unitSlotColor, -0.5f);
            selectBtn.colors = colors;
        }

        private RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, 
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject panelGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGO.transform.SetParent(parent, false);

            Image image = panelGO.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;

            return rect;
        }

        private Text CreateText(Transform parent, string content, int fontSize, TextAnchor anchor, Color color)
        {
            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
            textGO.transform.SetParent(parent, false);

            Text text = textGO.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.raycastTarget = false;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return text;
        }

        private Button CreateButton(RectTransform parent, string label, float xPercent)
        {
            GameObject btnGO = new GameObject(label + "Btn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);

            RectTransform rect = btnGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(xPercent, 0.2f);
            rect.anchorMax = new Vector2(xPercent + 0.25f, 0.8f);
            rect.offsetMin = new Vector2(5f, 5f);
            rect.offsetMax = new Vector2(-5f, -5f);

            Image image = btnGO.GetComponent<Image>();
            image.color = accentColor;
            image.raycastTarget = true;

            Button button = btnGO.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = accentColor;
            colors.highlightedColor = Lighten(accentColor, 0.2f);
            colors.pressedColor = Lighten(accentColor, -0.2f);
            button.colors = colors;

            Text btnText = CreateText(btnGO.transform, label.ToUpperInvariant(), 18, TextAnchor.MiddleCenter, Color.black);
            btnText.rectTransform.offsetMin = Vector2.zero;
            btnText.rectTransform.offsetMax = Vector2.zero;

            return button;
        }

        private Color Lighten(Color color, float amount)
        {
            if (amount >= 0f)
            {
                return Color.Lerp(color, Color.white, amount);
            }

            return Color.Lerp(color, Color.black, Mathf.Abs(amount));
        }

        private Color Darken(Color color, float amount)
        {
            return Color.Lerp(color, Color.black, Mathf.Abs(amount));
        }
    }
}

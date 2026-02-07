using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BattleYouCan
{
    /// <summary>
    /// Builds a Hub UI with multiple tabs and wires it to HubUIManager at runtime.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class HubUITabBuilder : MonoBehaviour
    {
        [SerializeField] private string[] tabNames = { "Home", "Units", "Research", "Armory" };
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.06f, 0.1f, 0.92f);
        [SerializeField] private Color panelColor = new Color(0.14f, 0.16f, 0.22f, 0.95f);
        [SerializeField] private Color contentColor = new Color(0.12f, 0.14f, 0.18f, 0.85f);
        [SerializeField] private Color accentColor = new Color(0.32f, 0.67f, 0.91f, 1f);
        [SerializeField] private Font fallbackFont;

        private bool built;

        private void OnEnable()
        {
            built = false;
            Build();
        }

        private void Start()
        {
            Build();
        }

        private void Build()
        {
            if (built)
            {
                return;
            }

            built = true;
            if (fallbackFont == null)
            {
                fallbackFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            Canvas canvas = EnsureCanvas();
            EnsureEventSystem();

            Transform existing = canvas.transform.Find("HubRuntimeRoot");
            if (existing != null)
            {
                return;
            }

            GameObject rootGO = new GameObject("HubRuntimeRoot", typeof(RectTransform));
            RectTransform root = rootGO.GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            // Background
            CreateImage(root, "Background", backgroundColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);

            // Header bar
            RectTransform header = CreateImage(root, "HeaderBar", panelColor, new Vector2(0f, 0.88f), new Vector2(1f, 1f), new Vector2(20f, -10f), new Vector2(-20f, -10f), false);
            Text title = CreateText(header, "COMMAND HUB", 30, TextAnchor.MiddleLeft, Color.white);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(0.35f, 1f);
            titleRect.offsetMin = new Vector2(20f, 0f);
            titleRect.offsetMax = new Vector2(-20f, 0f);

            // Content root
            RectTransform contentRoot = CreateImage(root, "ContentRoot", Color.clear, new Vector2(0f, 0f), new Vector2(1f, 0.88f), new Vector2(20f, 20f), new Vector2(-20f, -20f), false);

            // Manager
            HubUIManager manager = canvas.GetComponent<HubUIManager>();
            if (manager == null)
            {
                manager = canvas.gameObject.AddComponent<HubUIManager>();
            }

            var tabButtons = new List<HubUIManager.TabButton>();
            float tabWidth = 0.18f;
            float startAnchor = 0.42f;
            float gap = 0.11f;

            for (int i = 0; i < tabNames.Length; i++)
            {
                float min = startAnchor + gap * i;
                float max = min + tabWidth;
                RectTransform tab = CreateImage(header, $"Tab_{tabNames[i]}", panelColor, new Vector2(min, 0f), new Vector2(max, 1f), new Vector2(0f, -10f), new Vector2(0f, -10f), true);
                Button button = tab.gameObject.AddComponent<Button>();

                ColorBlock colors = button.colors;
                colors.normalColor = panelColor;
                colors.highlightedColor = Color.Lerp(panelColor, Color.white, 0.1f);
                colors.pressedColor = Color.Lerp(panelColor, Color.black, 0.2f);
                colors.selectedColor = accentColor;
                colors.disabledColor = new Color(panelColor.r, panelColor.g, panelColor.b, 0.5f);
                colors.colorMultiplier = 1f;
                button.colors = colors;

                Text label = CreateText(tab, tabNames[i].ToUpperInvariant(), 20, TextAnchor.MiddleCenter, Color.white);
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = Vector2.zero;

                RectTransform content = CreateImage(contentRoot, $"Content_{tabNames[i]}", contentColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);
                content.gameObject.SetActive(i == 0);
                Text placeholder = CreateText(content, $"{tabNames[i].ToUpperInvariant()} PANEL", 26, TextAnchor.MiddleCenter, Color.white);
                placeholder.rectTransform.offsetMin = Vector2.zero;
                placeholder.rectTransform.offsetMax = Vector2.zero;

                var tabButton = new HubUIManager.TabButton
                {
                    button = button,
                    content = content.gameObject,
                    activeColor = accentColor,
                    inactiveColor = panelColor
                };
                tabButtons.Add(tabButton);
            }

            manager.ConfigureTabs(tabButtons.ToArray(), 0);
        }

        private Canvas EnsureCanvas()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.isRootCanvas)
            {
                return canvas;
            }

            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas candidate in canvases)
            {
                if (candidate.isRootCanvas && candidate.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return candidate;
                }
            }

            GameObject canvasGO = new GameObject("HubUICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.layer = LayerMask.NameToLayer("UI");

            Canvas newCanvas = canvasGO.GetComponent<Canvas>();
            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return newCanvas;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetAsLastSibling();
        }

        private RectTransform CreateImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool raycast)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;

            Image img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;

            return rect;
        }

        private Text CreateText(Transform parent, string content, int fontSize, TextAnchor anchor, Color color)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);

            Text text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.font = fallbackFont;
            text.raycastTarget = false;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return text;
        }
    }
}

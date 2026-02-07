using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BattleYouCan
{
    /// <summary>
    /// Lightweight runtime builder that spawns a mock hub UI so the scene stays tidy in source control.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class HubUISampleUI : MonoBehaviour
    {
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.06f, 0.1f, 0.92f);
        [SerializeField] private Color panelColor = new Color(0.14f, 0.16f, 0.22f, 0.95f);
        [SerializeField] private Color accentColor = new Color(0.32f, 0.67f, 0.91f, 1f);
        [SerializeField] private Font fallbackFont;

        private bool initialized;

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
            if (fallbackFont == null)
            {
                fallbackFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            Canvas canvas = EnsureCanvas();
            EnsureEventSystem();

            if (canvas.transform.Find("HubSampleRoot") != null)
            {
                return;
            }

            BuildHubLayout(canvas.transform);
        }

        private Canvas EnsureCanvas()
        {
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

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
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

        private void BuildHubLayout(Transform canvasTransform)
        {
            RectTransform root = CreatePanel(canvasTransform, "HubSampleRoot", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, backgroundColor);
            CreateHeader(root);
            CreateSquadColumn(root);
            CreateOperationsColumn(root);
        }

        private void CreateHeader(RectTransform root)
        {
            RectTransform header = CreatePanel(root, "HeaderBar", new Vector2(0f, 0.85f), new Vector2(1f, 1f), new Vector2(30f, -20f), new Vector2(-30f, -20f), panelColor);

            Text title = CreateText(header, "Command Hub", 34, TextAnchor.MiddleLeft, Color.white);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(35f, 0f);
            titleRect.offsetMax = new Vector2(-35f, 0f);

            CreateStatBlock(header, "Credits", "124,500", new Vector2(0.5f, 0.5f), new Vector2(0.65f, 0.5f));
            CreateStatBlock(header, "Energy", "3,240", new Vector2(0.65f, 0.5f), new Vector2(0.8f, 0.5f));
            CreateStatBlock(header, "Intel", "87", new Vector2(0.8f, 0.5f), new Vector2(0.92f, 0.5f));

            string[] buttons = { "Battle", "Research", "Hangar" };
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = CreateButton(header, buttons[i]);
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f + (0.16f * i), 0.1f);
                rect.anchorMax = new Vector2(0.62f + (0.16f * i), 0.9f);
                rect.offsetMin = new Vector2(10f, 10f);
                rect.offsetMax = new Vector2(-10f, -10f);
            }
        }

        private void CreateSquadColumn(RectTransform root)
        {
            RectTransform panel = CreatePanel(root, "SquadPanel", new Vector2(0f, 0f), new Vector2(0.4f, 0.85f), new Vector2(30f, 30f), new Vector2(-15f, -40f), panelColor);
            CreateSectionTitle(panel, "Active Squad");

            RectTransform listContainer = CreateContentContainer(panel, new Vector2(20f, 30f), new Vector2(-20f, -30f));
            VerticalLayoutGroup layout = listContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.padding = new RectOffset(0, 0, 60, 20);

            string[] entries =
            {
                "Vanguard Alpha  |  Assault",
                "Medic Echo      |  Support",
                "Sniper Lynx     |  Recon",
                "Engineer Bolt   |  Utility"
            };

            foreach (string entry in entries)
            {
                Text row = CreateText(listContainer, entry, 22, TextAnchor.MiddleLeft, Color.Lerp(Color.white, accentColor, 0.15f));
                RectTransform rect = row.rectTransform;
                rect.sizeDelta = new Vector2(0f, 48f);
                Image underline = row.gameObject.AddComponent<Image>();
                underline.color = new Color(1f, 1f, 1f, 0.05f);
                underline.raycastTarget = false;
            }
        }

        private void CreateOperationsColumn(RectTransform root)
        {
            RectTransform panel = CreatePanel(root, "OperationsPanel", new Vector2(0.4f, 0f), new Vector2(1f, 0.85f), new Vector2(15f, 30f), new Vector2(-30f, -40f), panelColor);

            RectTransform missionPanel = CreatePanel(panel, "MissionPanel", new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(20f, -10f), new Vector2(-20f, -20f), Lighten(panelColor, 0.08f));
            CreateSectionTitle(missionPanel, "Priority Missions");
            PopulateList(missionPanel, new[]
            {
                "Secure Arcadia Relay  |  78% Intel",
                "Escort Supply Convoy  |  Ready",
                "Disrupt Signal Array  |  Requires Stealth"
            });

            Button launchButton = CreateButton(missionPanel, "Launch Mission");
            RectTransform launchRect = launchButton.GetComponent<RectTransform>();
            launchRect.anchorMin = new Vector2(0.65f, 0f);
            launchRect.anchorMax = new Vector2(1f, 0.2f);
            launchRect.offsetMin = new Vector2(-20f, 20f);
            launchRect.offsetMax = new Vector2(-20f, 60f);

            RectTransform resourcePanel = CreatePanel(panel, "ResourcePanel", new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(20f, 20f), new Vector2(-20f, -10f), Lighten(panelColor, 0.15f));
            CreateSectionTitle(resourcePanel, "Logistics");
            PopulateList(resourcePanel, new[]
            {
                "Manufacturing Queue  |  6 hrs",
                "Research Lab         |  Quantum Drives",
                "Docked Units         |  3/5 Ready"
            });
        }

        private RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
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

        private RectTransform CreateContentContainer(RectTransform parent, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject container = new GameObject(parent.name + "Content", typeof(RectTransform));
            container.transform.SetParent(parent, false);

            RectTransform rect = container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            return rect;
        }

        private Text CreateText(Transform parent, string content, int fontSize, TextAnchor anchor, Color color)
        {
            GameObject textGO = new GameObject(content.Replace(" ", string.Empty) + "Text", typeof(RectTransform), typeof(CanvasRenderer));
            textGO.transform.SetParent(parent, false);

            Text text = textGO.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.font = fallbackFont;
            text.raycastTarget = false;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return text;
        }

        private void CreateSectionTitle(RectTransform parent, string text)
        {
            Text title = CreateText(parent, text.ToUpperInvariant(), 26, TextAnchor.UpperLeft, Color.white);
            RectTransform rect = title.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(24f, -60f);
            rect.offsetMax = new Vector2(-24f, -20f);
        }

        private void PopulateList(RectTransform parent, string[] rows)
        {
            RectTransform container = CreateContentContainer(parent, new Vector2(20f, 20f), new Vector2(-20f, -90f));
            VerticalLayoutGroup layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.padding = new RectOffset(0, 0, 60, 0);

            foreach (string rowText in rows)
            {
                Text row = CreateText(container, rowText, 22, TextAnchor.MiddleLeft, Color.Lerp(Color.white, accentColor, 0.2f));
                RectTransform rect = row.rectTransform;
                rect.sizeDelta = new Vector2(0f, 44f);
            }
        }

        private void CreateStatBlock(RectTransform parent, string label, string value, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform block = CreatePanel(parent, label + "Block", anchorMin, anchorMax, new Vector2(0f, 10f), new Vector2(-20f, -10f), Lighten(panelColor, 0.05f));
            CreateText(block, label.ToUpperInvariant(), 16, TextAnchor.UpperLeft, Color.Lerp(Color.white, accentColor, 0.1f));

            Text valueText = CreateText(block, value, 24, TextAnchor.LowerLeft, Color.white);
            RectTransform rect = valueText.rectTransform;
            rect.offsetMin = new Vector2(20f, 10f);
            rect.offsetMax = new Vector2(-20f, -10f);
        }

        private Button CreateButton(Transform parent, string label)
        {
            GameObject buttonGO = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(parent, false);

            Image image = buttonGO.GetComponent<Image>();
            image.color = accentColor;
            image.raycastTarget = true;

            Button button = buttonGO.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.colorMultiplier = 1f;
            colors.highlightedColor = Lighten(accentColor, 0.2f);
            colors.pressedColor = Lighten(accentColor, -0.2f);
            colors.selectedColor = accentColor;
            colors.normalColor = accentColor;
            colors.disabledColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
            button.colors = colors;

            Text labelText = CreateText(buttonGO.transform, label.ToUpperInvariant(), 20, TextAnchor.MiddleCenter, Color.black);
            labelText.rectTransform.offsetMin = Vector2.zero;
            labelText.rectTransform.offsetMax = Vector2.zero;

            RectTransform rect = buttonGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180f, 60f);

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
    }
}

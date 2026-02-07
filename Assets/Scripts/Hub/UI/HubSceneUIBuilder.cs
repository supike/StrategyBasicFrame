using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Hub.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class HubSceneUIBuilder : MonoBehaviour
    {
        private bool built;

        // References to sub-builders (populated at runtime)
        private HubTopBarBuilder topBarBuilder;
        private HubLeftPanelBuilder leftPanelBuilder;
        private HubCenterPanelBuilder centerPanelBuilder;
        private HubBottomBarBuilder bottomBarBuilder;

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
            if (built) return;
            built = true;

            EnsureEventSystem();

            // Check ALL root canvases for existing HubSceneRoot to prevent duplicates
            foreach (var c in FindObjectsOfType<Canvas>())
            {
                if (c.isRootCanvas && c.transform.Find("HubSceneRoot") != null)
                    return;
            }

            Canvas canvas = EnsureCanvas();

            // Root
            GameObject rootGO = new GameObject("HubSceneRoot", typeof(RectTransform));
            RectTransform root = rootGO.GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            // Background
            HubUIHelper.CreatePanel(root, "Background",
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                HubUIHelper.BackgroundColor);

            // Top Bar (top 8%)
            RectTransform topBar = HubUIHelper.CreatePanel(root, "TopBar",
                new Vector2(0f, 0.92f), new Vector2(1f, 1f),
                new Vector2(8f, -4f), new Vector2(-8f, -4f),
                HubUIHelper.PanelColor);
            topBarBuilder = new HubTopBarBuilder();
            topBarBuilder.Build(topBar);

            // Left Panel (left 20%, between top bar and bottom bar)
            RectTransform leftPanel = HubUIHelper.CreatePanel(root, "LeftPanel",
                new Vector2(0f, 0.10f), new Vector2(0.20f, 0.915f),
                new Vector2(8f, 4f), new Vector2(-4f, -4f),
                HubUIHelper.PanelColor);
            leftPanelBuilder = new HubLeftPanelBuilder();
            leftPanelBuilder.Build(leftPanel, canvas);

            // Center Panel (right 80%, between top bar and bottom bar)
            RectTransform centerPanel = HubUIHelper.CreatePanel(root, "CenterPanel",
                new Vector2(0.20f, 0.10f), new Vector2(1f, 0.915f),
                new Vector2(4f, 4f), new Vector2(-8f, -4f),
                Color.clear);
            centerPanelBuilder = new HubCenterPanelBuilder();
            centerPanelBuilder.Build(centerPanel);

            // Bottom Bar (bottom 10%)
            RectTransform bottomBar = HubUIHelper.CreatePanel(root, "BottomBar",
                new Vector2(0f, 0f), new Vector2(1f, 0.10f),
                new Vector2(8f, 4f), new Vector2(-8f, -2f),
                HubUIHelper.PanelColor);
            bottomBarBuilder = new HubBottomBarBuilder();
            bottomBarBuilder.Build(bottomBar);
        }

        private Canvas EnsureCanvas()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas candidate in canvases)
            {
                if (candidate.isRootCanvas && candidate.renderMode == RenderMode.ScreenSpaceOverlay)
                    return candidate;
            }

            GameObject canvasGO = new GameObject("HubCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            if (FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}

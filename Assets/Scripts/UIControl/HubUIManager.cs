using System;
using UnityEngine;
using UnityEngine.UI;

namespace BattleYouCan
{
    /// <summary>
    /// Manages the hub UI tab system and navigation.
    /// </summary>
    public class HubUIManager : MonoBehaviour
    {
        [System.Serializable]
        public class TabButton
        {
            public Button button;
            public GameObject content;
            public Color activeColor = new Color(0.32f, 0.67f, 0.91f, 1f);
            public Color inactiveColor = new Color(0.14f, 0.16f, 0.22f, 1f);
        }

        [SerializeField] private TabButton[] tabs = new TabButton[0];
        [SerializeField] private int defaultTabIndex = 0;

        private int activeTabIndex = -1;

        private void Start()
        {
            if (tabs.Length > 0 && activeTabIndex < 0)
            {
                InitializeTabs();
                SelectTab(defaultTabIndex);
            }
        }

        public void ConfigureTabs(TabButton[] newTabs, int defaultIndex = 0)
        {
            tabs = newTabs ?? Array.Empty<TabButton>();
            defaultTabIndex = Mathf.Clamp(defaultIndex, 0, tabs.Length > 0 ? tabs.Length - 1 : 0);
            activeTabIndex = -1;
            InitializeTabs();
            SelectTab(defaultTabIndex);
        }

        private void InitializeTabs()
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i;
                if (tabs[i].button != null)
                {
                    tabs[i].button.onClick.AddListener(() => SelectTab(index));
                }
            }
        }

        public void SelectTab(int tabIndex)
        {
            if (tabIndex == activeTabIndex || tabIndex < 0 || tabIndex >= tabs.Length)
            {
                return;
            }

            // Deactivate previous tab
            if (activeTabIndex >= 0)
            {
                if (tabs[activeTabIndex].content != null)
                {
                    tabs[activeTabIndex].content.SetActive(false);
                }
                if (tabs[activeTabIndex].button != null)
                {
                    Image btnImage = tabs[activeTabIndex].button.GetComponent<Image>();
                    if (btnImage != null)
                    {
                        btnImage.color = tabs[activeTabIndex].inactiveColor;
                    }
                }
            }

            // Activate new tab
            activeTabIndex = tabIndex;
            if (tabs[tabIndex].content != null)
            {
                tabs[tabIndex].content.SetActive(true);
            }
            if (tabs[tabIndex].button != null)
            {
                Image btnImage = tabs[tabIndex].button.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = tabs[tabIndex].activeColor;
                }
            }
        }

        public int GetActiveTabIndex()
        {
            return activeTabIndex;
        }
    }
}

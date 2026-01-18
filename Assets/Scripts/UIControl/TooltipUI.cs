using TMPro;
using UnityEngine;

namespace BattleYouCan
{
    public class TooltipUI : MonoBehaviour
    {
        public static TooltipUI Instance;
        public RectTransform panel;
        public TMP_Text text;

        void Awake() => Instance = this;

        public void Show(string msg)
        {
            text.text = msg;
            panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            panel.gameObject.SetActive(false);
        }

        void Update()
        {
            panel.position = Input.mousePosition;
        }
    }
}
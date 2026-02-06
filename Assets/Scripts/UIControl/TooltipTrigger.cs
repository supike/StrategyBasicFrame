using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace BattleYouCan
{
    public class TooltipTrigger: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea]
        public string tooltipText;
        private float showDuration = .1f; 
        
        private Coroutine autoHideCoroutine;


        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipUI.Instance.Show(tooltipText);
            autoHideCoroutine = StartCoroutine(AutoHideAfter(showDuration));

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // TooltipUI.Instance.Hide();
        }
        
        private IEnumerator AutoHideAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            TooltipUI.Instance.Hide();
            autoHideCoroutine = null;
        }
    }
}
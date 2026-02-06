using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BattleYouCan
{
    [System.Serializable]
    public struct AttributeUI
    {
        public TMP_Text label;
        public Slider slider;
        public TMP_Text valueText;
    }
    
    public class UICharacterDetail : MonoBehaviour
    {
        public static UICharacterDetail Instance;

        public Image portrait;
        public TMP_Text nameText;
        public TMP_Text classText;

        public AttributeUI speed;
        public AttributeUI technique;
        public AttributeUI intelligence;
        public AttributeUI mentality;
        public AttributeUI stamina;

        void Awake()
        {
            Instance = this;
            gameObject.SetActive(false);
        }

        public void Show(CharacterData data)
        {
            gameObject.SetActive(true);

            portrait.sprite = data.iconImage;
            nameText.text = data.characterName;
            // classText.text = data.unitClass.ToString();

            BindAttribute(speed, data.speed);
            BindAttribute(technique, data.sight);
            BindAttribute(intelligence, data.intel);
            BindAttribute(mentality, data.agression);
            BindAttribute(stamina, data.stamina);
        }

        void BindAttribute(AttributeUI ui, int value)
        {
            ui.slider.maxValue = 20; // FM 스타일
            ui.slider.value = value;
            ui.valueText.text = value.ToString();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }

}
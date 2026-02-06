using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleYouCan
{
    public class UnitListUI : MonoBehaviour
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text classText;

        private CharacterData unitData;

        public void Bind(CharacterData data)
        {
            unitData = data;
            icon.sprite = data.iconImage;
            nameText.text = data.characterName;
            // classText.text = data.unitClass.ToString();
        }

        public void OnClick()
        {
            UICharacterDetail.Instance.Show(unitData);
        }
    }

}
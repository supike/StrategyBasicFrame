using System.Collections.Generic;
using UnityEngine;

namespace BattleYouCan
{
    public class UICharacterList : MonoBehaviour
    {
        public Transform content;
        public UnitListUI itemPrefab;
        public List<CharacterData> units;

        void Start()
        {
            foreach (var unit in units)
            {
                var item = Instantiate(itemPrefab, content);
                item.Bind(unit);
            }
        }
    }

}
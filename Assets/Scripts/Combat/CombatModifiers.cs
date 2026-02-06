using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class CombatModifiers: MonoBehaviour
    {
        // 유닛 타입별 상성표
        private static Dictionary<(EClass attacker, EClass defender), float> _typeAdvantages = 
            new Dictionary<(EClass, EClass), float>
            {
                // 기병 > 궁수
                { (EClass.Mounted, EClass.Ranged), 1.5f },
                // 창병(보병) > 기병
                { (EClass.Infantry, EClass.Mounted), 1.5f },
                // 궁수 > 보병
                { (EClass.Ranged, EClass.Infantry), 1.3f },
                // 역상성
                { (EClass.Ranged, EClass.Mounted), 0.7f },
                { (EClass.Mounted, EClass.Infantry), 0.7f },
                { (EClass.Infantry, EClass.Ranged), 0.8f }
            };
    
        public static float GetTypeAdvantage(EClass attacker, EClass defender)
        {
            if (_typeAdvantages.TryGetValue((attacker, defender), out float modifier))
                return modifier;
            return 1f; // 중립
        }
        
        
        // static float GetTerrainBonus(Tile attackerTile, Tile defenderTile)
        // {
        //     // 고지에서 공격 시 보너스
        //     if (attackerTile.Height > defenderTile.Height)
        //     {
        //         return 1.2f; // +20%
        //     }
        //     // 저지에서 공격 시 페널티
        //     else if (attackerTile.Height < defenderTile.Height)
        //     {
        //         return 0.8f; // -20%
        //     }
        //
        //     return 1f;
        // }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class CombatModifiers : MonoBehaviour
    {
        // 유닛 타입별 상성표
        private static Dictionary<(UnitType attacker, UnitType defender), float> typeAdvantages = 
            new Dictionary<(UnitType, UnitType), float>
            {
                // 기병 > 궁수
                { (UnitType.Mounted, UnitType.Ranged), 1.5f },
                // 창병 > 기병
                { (UnitType.Infantry, UnitType.Mounted), 1.5f },
                // 궁수 > 보병
                { (UnitType.Ranged, UnitType.Infantry), 1.3f },
                // 역상성
                { (UnitType.Ranged, UnitType.Mounted), 0.7f },
                { (UnitType.Mounted, UnitType.Infantry), 0.7f },
                { (UnitType.Infantry, UnitType.Ranged), 0.8f }
            };
    
        public static float GetTypeAdvantage(UnitType attacker, UnitType defender)
        {
            if (typeAdvantages.TryGetValue((attacker, defender), out float modifier))
                return modifier;
            return 1f; // 중립
        }
        
        
        static float GetTerrainBonus(Tile attackerTile, Tile defenderTile)
        {
            // 고지에서 공격 시 보너스
            if (attackerTile.Height > defenderTile.Height)
            {
                return 1.2f; // +20%
            }
            // 저지에서 공격 시 페널티
            else if (attackerTile.Height < defenderTile.Height)
            {
                return 0.8f; // -20%
            }
        
            return 1f;
        }
    }
}
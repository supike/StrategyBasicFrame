using UnityEngine;
namespace Combat
{
    public class CombatBasic: MonoBehaviour
    {
        public static int CalculateSubtractionDamage(Unit attacker, Unit defender)
        {
            int rawDamage = attacker.stats.baseAttack;
            int defense = defender.stats.physicalDefense;
        
            // 공격력 - 방어력
            int finalDamage = Mathf.Max(1, rawDamage - defense);
        
            return finalDamage;
        }
        // 실제 전투에서 사용되는 함수
        public static int EngageBattle(Unit attacker, Unit defender)
        {
            int rawDamage = attacker.stats.baseAttack;
            int defense = defender.stats.physicalDefense;
        
            // 공격력 - 방어력
            int finalDamage = Mathf.Max(1, rawDamage - defense);
        
            return finalDamage;
        }
    }
}
using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public class StatGrowth
    {
        public int healthGrowth = 10;     // 레벨당 HP 증가
        public int attackGrowth = 2;      // 레벨당 공격력 증가
        public int defenseGrowth = 1;     // 레벨당 방어력 증가
    }

    public class LevelingSystem: MonoBehaviour
    {
        static public LevelingSystem Instance { get; private set; }
        [SerializeField] private StatGrowth growthRates;
    
        public void GainExperience(Unit unit, int exp)
        {
            unit.stats.experience += exp;
        
            int expRequired = CalculateExpForNextLevel(unit.stats.level);
        
            if (unit.stats.experience >= expRequired)
            {
                LevelUp(unit);
            }
        }
    
        void LevelUp(Unit unit)
        {
            unit.stats.level++;
            unit.stats.experience = 0;
        
            // 스탯 증가
            unit.stats.maxHealth += growthRates.healthGrowth;
            unit.stats.currentHealth = unit.stats.maxHealth;
            unit.stats.baseAttack += growthRates.attackGrowth;
            unit.stats.physicalDefense += growthRates.defenseGrowth;
        
            Debug.Log($"{unit.name} leveled up to {unit.stats.level}!");
        }
    
        int CalculateExpForNextLevel(int currentLevel)
        {
            return currentLevel * 100; // 레벨 * 100 경험치
        }
    }
}
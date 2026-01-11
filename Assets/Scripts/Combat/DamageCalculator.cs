using UnityEngine;

namespace Combat
{
public enum DamageType
{
    Physical,
    Magical,
    True,      // 방어 무시
    Poison
}

public class DamageCalculator: MonoBehaviour
{
    // 방법 1: 감산 방식 (Fire Emblem 스타일)
    public static int CalculateSubtractionDamage(Unit attacker, Unit defender)
    {
        int rawDamage = attacker.stats.baseAttack;
        int defense = defender.stats.physicalDefense;
        
        // 공격력 - 방어력
        int finalDamage = Mathf.Max(1, rawDamage - defense);
        
        return finalDamage;
    }
    
    // 방법 2: 비율 방식 (Pokemon Unite 스타일)
    public static int CalculateRatioDamage(Unit attacker, Unit defender, int skillPower)
    {
        // 공격 데미지 = 공격력 + 스킬 위력
        int attackDamage = attacker.stats.baseAttack + skillPower;
        
        // 방어 비율 = 600 / (600 + 방어력)
        float defenseRatio = 600f / (600f + defender.stats.physicalDefense);
        
        // 최종 데미지 = 공격 데미지 * 방어 비율
        int finalDamage = Mathf.RoundToInt(attackDamage * defenseRatio);
        
        return Mathf.Max(1, finalDamage);
    }
    
    // 방법 3: 나눗셈 방식 (Pokemon/Final Fantasy 스타일)
    public static int CalculateDivisionDamage(Unit attacker, Unit defender, int skillPower)
    {
        // 데미지 = (공격력^2 / 방어력) * 스킬 위력
        float attackSquared = attacker.stats.baseAttack * attacker.stats.baseAttack;
        float baseDamage = attackSquared / (float)defender.stats.physicalDefense;
        
        int finalDamage = Mathf.RoundToInt(baseDamage * skillPower / 10f);
        
        return Mathf.Max(1, finalDamage);
    }
    
    // 추천: 하이브리드 방식
    public static int CalculateDamage(Unit attacker, Unit defender, DamageType damageType, int skillPower = 0)
    {
        // 1. 기본 데미지 계산
        int baseDamage = attacker.stats.baseAttack;
        
        // 2. 스킬 위력 추가
        if (skillPower > 0)
        {
            baseDamage += skillPower;
        }
        
        // 3. 데미지 랜덤성 적용 (±10%)
        int variation = Random.Range(-baseDamage / 10, baseDamage / 10);
        baseDamage += variation;
        
        // 4. 방어력에 따른 데미지 감소
        float damageMultiplier = 1f;
        
        if (damageType == DamageType.Physical)
        {
            // 방어력 비율: 100 / (100 + 방어력)
            damageMultiplier = 100f / (100f + defender.stats.physicalDefense);
        }
        else if (damageType == DamageType.Magical)
        {
            damageMultiplier = 100f / (100f + defender.stats.magicalDefense);
        }
        else if (damageType == DamageType.True)
        {
            damageMultiplier = 1f; // 방어 무시
        }
        
        int reducedDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
        // 전투 모드에 따른 데미지 조정.
        if (defender.stats.unitMode == UnitMode.Defence)
        {
            reducedDamage = Mathf.RoundToInt(reducedDamage * .5f);
        }

        // 5. 유닛 타입 상성 적용
        float typeAdvantage = CombatModifiers.GetTypeAdvantage(
            attacker.unitData.unitType, 
            defender.unitData.unitType
        );
        reducedDamage = Mathf.RoundToInt(reducedDamage * typeAdvantage);
        
        // 6. 지형 보너스 적용
        float terrainBonus = GetTerrainBonus(attacker.CurrentTile, defender.CurrentTile);
        reducedDamage = Mathf.RoundToInt(reducedDamage * terrainBonus);
        
        // 7. 치명타 계산
        if (Random.value < attacker.unitData.criticalChance)
        {
            reducedDamage = Mathf.RoundToInt(reducedDamage * attacker.unitData.criticalMultiplier);
            CombatEventSystem.Instance.TriggerEvent(new CombatEvent
            {
                eventType = CombatEventType.CriticalHit,
                attacker = attacker,
                defender = defender
            });
        }
        
        // 8. 최소 데미지 보장
        return Mathf.Max(1, reducedDamage);
    }
    
    static float GetTerrainBonus(TileCustomWithEvent attackerTile, TileCustomWithEvent defenderTile)
    {
        // 고지에서 공격 시 보너스
        // if (attackerTile.Height > defenderTile.Height)
        // {
        //     return 1.2f; // +20%
        // }
        // // 저지에서 공격 시 페널티
        // else if (attackerTile.Height < defenderTile.Height)
        // {
        //     return 0.8f; // -20%
        // }
        
        return 1f;
    }
}
}
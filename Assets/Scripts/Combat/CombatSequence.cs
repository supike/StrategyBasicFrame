using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
public class CombatSequence : MonoBehaviour
{
    private Queue<CombatAction> actionQueue = new Queue<CombatAction>();
    private bool isExecuting = false;
    
    public void QueueCombatAction(CombatAction action)
    {
        actionQueue.Enqueue(action);
        
        if (!isExecuting)
        {
            StartCoroutine(ExecuteCombatSequence());
        }
    }
    
    IEnumerator ExecuteCombatSequence()
    {
        isExecuting = true;
        
        while (actionQueue.Count > 0)
        {
            CombatAction action = actionQueue.Dequeue();
            
            yield return StartCoroutine(ExecuteAction(action));
        }
        
        isExecuting = false;
    }
    
    IEnumerator ExecuteAction(CombatAction action)
    {
        Unit attacker = action.attacker;
        Unit defender = action.defender;
        
        // 1. 공격 애니메이션 시작
        attacker.PlayAnimation("Attack");
        CombatEventSystem.Instance.TriggerEvent(new CombatEvent
        {
            eventType = CombatEventType.AttackStarted,
            attacker = attacker,
            defender = defender
        });
        
        yield return new WaitForSeconds(0.5f);
        
        // 2. 회피 판정
        if (Random.value < defender.unitData.dodgeChance)
        {
            CombatEventSystem.Instance.TriggerEvent(new CombatEvent
            {
                eventType = CombatEventType.AttackDodged,
                attacker = attacker,
                defender = defender,
                message = $"{defender.name} dodged the attack!"
            });
            
            yield return new WaitForSeconds(0.5f);
            yield break;
        }
        
        // 3. 데미지 계산
        int damage = DamageCalculator.CalculateDamage(
            attacker, 
            defender, 
            DamageType.Physical
        );
        
        // 4. 데미지 적용
        defender.TakeDamage(damage, DamageType.Physical);
        
        // 5. 데미지 이벤트 발생
        CombatEventSystem.Instance.TriggerEvent(new CombatEvent
        {
            eventType = CombatEventType.DamageDealt,
            attacker = attacker,
            defender = defender,
            damage = damage,
            message = $"{attacker.name} dealt {damage} damage to {defender.name}!"
        });
        
        // 6. 피격 애니메이션
        defender.PlayAnimation("Hit");
        
        yield return new WaitForSeconds(0.3f);
        
        // 7. 사망 확인
        if (!defender.IsAlive)
        {
            CombatEventSystem.Instance.TriggerEvent(new CombatEvent
            {
                eventType = CombatEventType.UnitDefeated,
                attacker = attacker,
                defender = defender,
                message = $"{defender.name} has been defeated!"
            });
            
            defender.PlayAnimation("Death");
            yield return new WaitForSeconds(1f);
            
            // 경험치 부여
            LevelingSystem.Instance.GainExperience(attacker, 50);
        }
        
        // 8. 반격 판정 (방어자가 생존하고 사거리 내라면)
        if (defender.IsAlive && CanCounterAttack(defender, attacker))
        {
            yield return new WaitForSeconds(0.5f);
            
            CombatEventSystem.Instance.TriggerEvent(new CombatEvent
            {
                eventType = CombatEventType.CounterAttackStarted,
                attacker = defender,
                defender = attacker
            });
            
            // 반격 실행
            yield return StartCoroutine(ExecuteCounterAttack(defender, attacker));
        }
    }
    
    IEnumerator ExecuteCounterAttack(Unit counterAttacker, Unit originalAttacker)
    {
        counterAttacker.PlayAnimation("Attack");
        yield return new WaitForSeconds(0.5f);
        
        int counterDamage = DamageCalculator.CalculateDamage(
            counterAttacker, 
            originalAttacker, 
            DamageType.Physical
        ) / 2; // 반격은 50% 데미지
        
        originalAttacker.TakeDamage(counterDamage, DamageType.Physical);
        
        CombatEventSystem.Instance.TriggerEvent(new CombatEvent
        {
            eventType = CombatEventType.CounterAttackHit,
            attacker = counterAttacker,
            defender = originalAttacker,
            damage = counterDamage,
            message = $"{counterAttacker.name} countered for {counterDamage} damage!"
        });
        
        originalAttacker.PlayAnimation("Hit");
        yield return new WaitForSeconds(0.5f);
    }
    
    bool CanCounterAttack(Unit defender, Unit attacker)
    {
        int distance = Mathf.Abs(defender.CurrentTile.X - attacker.CurrentTile.X) +
                      Mathf.Abs(defender.CurrentTile.Y - attacker.CurrentTile.Y);
        
        return distance <= defender.unitData.attackRange;
    }
}

[System.Serializable]
public class CombatAction
{
    public Unit attacker;
    public Unit defender;
    public int skillId = -1; // -1 = 기본 공격
}
}
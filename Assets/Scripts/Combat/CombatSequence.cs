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
        
        // if (!isExecuting)
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

        
        // 근거리 공격 범위 확인 및 이동
        // if (!IsAdjacent(attacker, defender))
        while (!IsAdjacent(attacker, defender))
        {
            Debug.Log($"{attacker.name} is not adjacent to {defender.name}. Moving closer...");
            
            // 방어자에게 가장 가까운 인접 타일 찾기
            TileCustomWithEvent targetTile = FindClosestAdjacentTile(attacker, defender);
            
            if (targetTile != null /*&& attacker.CanMove()*/)
            {
                // 이동 애니메이션 실행
                yield return StartCoroutine(attacker.MoveTo(targetTile));
                yield return new WaitForSeconds(0.0f);
            }
            else
            {
                Debug.LogWarning($"{attacker.name} cannot move to attack {defender.name}!");
                yield break; // 이동 불가능하면 공격 취소
            }
        }

        bool endCombat = false;
        while (!endCombat)
        {
            if (defender.isMoving)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
                
            
            // 공격 준비 쿨 타임을 기다림`
            yield return attacker.PrepareForAttack();
        
            DamagePopUpGenerator.Instance.CreateDamagePopUp(attacker.transform.position, "Attack!", DamageType.True);
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
                defender.Dodge();
                yield return new WaitForSeconds(0.5f);
                continue; // 다음 공격으로 넘어감
            }
        
            // 3. 데미지 계산
            int damage = DamageCalculator.CalculateDamage(
                attacker, 
                defender, 
                DamageType.Physical
            );
        
            // 4. 데미지 적용
            defender.TakeDamage(damage);
        
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
            // 사망 확인, 전투 종료 판정
            endCombat = defender.CurrentHealth <= 0;
            
        }
        // 0. 전투 준비 (공격 속도에 따른 대기 시간 등)

        //
        // // 7. 사망 확인
        // if (!defender.IsAlive)
        // {
        //     CombatEventSystem.Instance.TriggerEvent(new CombatEvent
        //     {
        //         eventType = CombatEventType.UnitDefeated,
        //         attacker = attacker,
        //         defender = defender,
        //         message = $"{defender.name} has been defeated!"
        //     });
        //     
        //     defender.PlayAnimation("Death");
        //     yield return new WaitForSeconds(1f);
        //     
        //     // 경험치 부여
        //     LevelingSystem.Instance.GainExperience(attacker, 50);
        // }
        //
        // // 8. 반격 판정 (방어자가 생존하고 사거리 내라면)
        // if (defender.IsAlive && CanCounterAttack(defender, attacker))
        // {
        //     yield return new WaitForSeconds(0.5f);
        //     
        //     CombatEventSystem.Instance.TriggerEvent(new CombatEvent
        //     {
        //         eventType = CombatEventType.CounterAttackStarted,
        //         attacker = defender,
        //         defender = attacker
        //     });
        //     
        //     // 반격 실행
        //     yield return StartCoroutine(ExecuteCounterAttack(defender, attacker));
        // }
    }
    
    /// <summary>
    /// 두 유닛이 인접한지 확인 (헥사곤 그리드 기준)
    /// </summary>
    bool IsAdjacent(Unit unit1, Unit unit2)
    {
        if (unit1.CurrentTile == null || unit2.CurrentTile == null)
            return false;
        
        // List<TileCustomWithEvent> neighbors = GridManager.Instance.GetNeighbors(unit1.CurrentTile);
        List<TileCustomWithEvent> neighbors = GridManager.Instance.GetNeighbors(new Vector3Int(unit1.CurrentTile.X, unit1.CurrentTile.Y));

        foreach (TileCustomWithEvent cusTile in neighbors)
        {
            if (cusTile.X == unit2.CurrentTile.X && cusTile.Y == unit2.CurrentTile.Y)
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// 공격자가 방어자에게 가장 가까운 인접 타일 찾기
    /// </summary>
    TileCustomWithEvent FindClosestAdjacentTile(Unit attacker, Unit defender)
    {
        if (defender.CurrentTile == null)
            return null;
        
        // 방어자 주변의 인접 타일들 가져오기
        // List<TileCustomWithEvent> adjacentTiles = GridManager.Instance.GetNeighbors(new Vector3Int(defender.CurrentTile.X, defender.CurrentTile.Y));
        
        TileCustomWithEvent closestTile = null;
        float minDistance = float.MaxValue;
        
        // foreach (Vector3Int tile in adjacentTiles)
        // {
            // TileData tileCustome = GridManager.Instance.GetTileDataAtCellPosition(new Vector3Int(tile.x, tile.y, 0));
            // 비어있는 타일만 고려
            // if (tile.OccupyingUnit != null && tile.OccupyingUnit != attacker)
            //     continue;
            // todo: 중복 이동 문제 해결하자.
            // 공격자의 이동 가능한 타일인지 확인
            List<TileCustomWithEvent> movableTiles = attacker.GetMovableTiles();
            foreach (TileCustomWithEvent tileCustomeMovable in movableTiles)
            {
                int distance = Mathf.Abs(tileCustomeMovable.X - defender.CurrentTile.X) + 
                               Mathf.Abs(tileCustomeMovable.Y - defender.CurrentTile.Y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTile = tileCustomeMovable;
                }
            }
            // if (!movableTiles.Contains(tile) && tile != attacker.CurrentTile)
            //     continue;
            //
            // // 공격자로부터의 거리 계산
            // int distance = Mathf.Abs(tile.X - attacker.CurrentTile.X) + 
            //               Mathf.Abs(tile.Y - attacker.CurrentTile.Y);
            //
            // if (distance < minDistance)
            // {
            //     minDistance = distance;
            //     closestTile = tile;
            // }
        // }
        
        return closestTile;
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
        
        //originalAttacker.TakeDamage(counterDamage, DamageType.Physical);
        originalAttacker.TakeDamage(counterDamage);
        
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
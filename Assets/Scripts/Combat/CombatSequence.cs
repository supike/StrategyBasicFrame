using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
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


            bool endCombat = false;

            // 전투 로직
            while (!endCombat)
            {

                // 1. 행동 준비 쿨 타임을 기다림`
                yield return attacker.PrepareForAction();

                // 2. BTS? 판단(공격, 수비, 후퇴, 등 전체적인 행동 선택)
                // attacker.DecideActionMode();

                // if (attacker.stats.unitMode == UnitMode.Normal || attacker.stats.unitMode == UnitMode.Attack)
                // {
                //     // 노멀과 공격 모드는 상대를 찾아 이동 및 공격 시도. 그냥 아래 루틴 실행 하면 됨.
                // }
                if (attacker.stats.unitMode == UnitMode.Defence)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }
                else if (attacker.stats.unitMode == UnitMode.Retreat)
                {
                    attacker.Retreat();
                    // Debug.Log($"{attacker.name} is in Retreat mode and will not move to attack.");
                    yield return new WaitForSeconds(1f);
                    // yield break; // 후퇴 모드면 공격 취소
                    break;
                }

                // 3. 목표 타겟팅 확인 (유효한 타겟인지, 사거리 내인지 등)
                bool isAdjacent = IsAdjacent(attacker, defender);


                // 근거리 공격 범위 확인 및 이동
                if (!isAdjacent)
                {
                    // 먼저 근처 주변 적이 있는지 확인. 없으면 기존 목표 적을 정보 확인한다.
                    Unit newEnermy = FindNearestPlayerUnit(attacker);
                    if (newEnermy != null && newEnermy != defender)
                    {
                        // 적 타겟팅 변경
                        QueueCombatAction(new CombatAction() { attacker = attacker, defender = newEnermy });
                        endCombat = true;
                        break;
                    }


                    Debug.Log($"{attacker.name} is not adjacent to {defender.name}. Moving closer...");

                    // 방어자를 향하는 가장 가까운 인접 타일 찾기
                    TileCustomWithEvent targetTile = FindClosestAdjacentTile(attacker, defender);

                    if (targetTile != null /*&& attacker.CanMove()*/)
                    {

                        // 이동 애니메이션 실행
                        yield return StartCoroutine(attacker.MoveTo(targetTile));
                        // yield return new WaitForSeconds(1f);

                    }
                    else
                    {
                        Debug.LogWarning($"{attacker.name} cannot move to attack {defender.name}!");
                        yield break; // 이동 불가능하면 공격 취소
                    }
                }
                else
                {
                    Debug.Log($"{attacker.name} is adjacent to {defender.name} and can attack.");
                    ////
                    /// 각종 요소들의 조합을 판단하는 곳. 공격 이외의 다른 행동들을 위함.
                    /// 자세/균형, 심리상태, 피로도, 전투 텐션 등에 의해 영향 받음.
                    // 1.체력이 절반이하이면 방어 모드로 전환.(고정)
                    if (attacker.IsHalfHealth())
                    {
                        attacker.SetBattleMode(UnitMode.Defence);
                    }

                    // 2.스테미너가 절반 이하면 공격시도 확률 줄임.(고정) ==> 수비를 위해 잠시 쉬거나 다른 행동(공격하지 않음 확률 증가).
                    if (attacker.CurrentStamina < (attacker.MaxStamina) / 2)
                    {
                        //1. 공격시도 확률이 줌.
                        if (attacker.CurrentStamina < 4 || attacker.CurrentStamina < Random.Range(0, attacker.MaxStamina))
                        {
                            // attacker.RecoverStamina();
                            attacker.doingCombo = false;
                            continue;
                        }
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /// /////////////// 액션 행동 영역 구간 ///////////////////////////////////////////////////////////////////////////////////////
                    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                    if (!defender.IsAlive || defender == null || attacker.targetUnit == null)
                    {
                        yield break;
                    }


                    // // 테스트 영역
                    // attacker.Retreat();
                    // yield return new WaitForSeconds(1f);
                    // continue;

                    // 1. 공격 애니메이션 시작
                    attacker.DecreaseStamina();
                    attacker.PlayAnimation("Attack");
                    CombatEventSystem.Instance.TriggerEvent(new CombatEvent
                    {
                        eventType = CombatEventType.AttackStarted,
                        attacker = attacker,
                        defender = defender
                    });

                    yield return new WaitForSeconds(0.5f);


                    // 2. 회피 판정 //다른 항목으로 결정될 수 있도록 수정하자.
                    float attackSuccessRate = attacker.CurrentBalance / (float)(attacker.CurrentBalance + defender.CurrentBalance);
                    if (Random.value > attackSuccessRate)
                    {
                        CombatEventSystem.Instance.TriggerEvent(new CombatEvent
                        {
                            eventType = CombatEventType.AttackDodged,
                            attacker = attacker,
                            defender = defender,
                            message = $"{defender.name} dodged the attack!"
                        });
                        defender.Dodge();
                        continue; // 다음 공격으로 넘어감
                    }

                    // 3. 데미지 계산
                    int damage = DamageCalculator.CalculateDamage(
                        attacker,
                        defender,
                        DamageType.Physical
                    );

                    // 4. 먼저 밸런스 적용.
                    bool isAlive = defender.TakeBalance(damage);

                    // 4. 데미지 적용
                    // bool isAlive = defender.TakeDamage(damage);

                    // 5. 데미지 이벤트 발생
                    CombatEventSystem.Instance.TriggerEvent(new CombatEvent
                    {
                        eventType = CombatEventType.DamageDealt,
                        attacker = attacker,
                        defender = defender,
                        damage = damage,
                        message = $"{attacker.name} dealt {damage} damage to {defender.name}!"
                    });

                    // 6. 사망 확인
                    if (!isAlive)
                    {
                        defender.PlayAnimation("Die");
                        yield return new WaitForSeconds(1f);
                        Destroy(defender.gameObject);
                        attacker.targetUnit = null;
                    }

                    // 사망 확인, 전투 종료 판정
                    endCombat = defender.CurrentHealth <= 0;
                }

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
        /// 처음부터 고정된 두 유닛이 인접한지를 확인.
        /// 더 인접한 다른 유닛이 있으면 타겟으로 변경.
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

                // if (cusTile.OccupyingUnit != null)
                // {
                //     // 다른 인접한 유닛이 있으면 목표 변경
                //     unit2 = cusTile.OccupyingUnit;
                //     return true;
                // }
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

                if (distance == minDistance)
                {
                    if (closestTile.X > tileCustomeMovable.X)
                    {
                        closestTile = tileCustomeMovable;
                    }
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

            // if (closestTile != defender.CurrentTile)
            // {
            //     return FindClosestAdjacentTile(, defender);
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

        // bool CanCounterAttack(Unit defender, Unit attacker)
        // {
        //     int distance = Mathf.Abs(defender.CurrentTile.X - attacker.CurrentTile.X) +
        //                   Mathf.Abs(defender.CurrentTile.Y - attacker.CurrentTile.Y);
        //     
        //     return distance <= defender.unitData.attackRange;
        // }

        public Unit FindNearestPlayerUnit(Unit unit1)
        {
            Unit nearestUnit = null;
            float minDistance = float.MaxValue;

            List<TileCustomWithEvent> neighbors = GridManager.Instance.GetNeighbors(new Vector3Int(unit1.CurrentTile.X, unit1.CurrentTile.Y));

            foreach (TileCustomWithEvent cusTile in neighbors)
            {
                if (cusTile.OccupyingUnit != null && cusTile.OccupyingUnit.playerUnit != unit1.playerUnit)
                {
                    // 다른 인접한 유닛이 있으면 목표 변경
                    return cusTile.OccupyingUnit;
                }
            }

            return null;
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
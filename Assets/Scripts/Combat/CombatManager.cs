using System;
using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public class CombatManager : MonoBehaviour
    {
        static public CombatManager Instance { get; private set; }
        private CombatSequence combatSequence;
        private CombatEventSystem combatEventSystem;
        [SerializeField]private Unit playerUnit;
        [SerializeField]private Unit[] enermyUnits;
        public GameObject PauseUI;
        
        // 모든 유닛 리스트
        private Unit[] allPlayerUnits;
        private Unit[] allEnemyUnits;

        public Unit[] GetAllPlayerUnits() => allPlayerUnits;
        public Unit[] GetAllEnemyUnits() => allEnemyUnits;
        
        private void Awake()
        {
            // Singleton 패턴
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            combatSequence = GetComponent<CombatSequence>();
            combatEventSystem = CombatEventSystem.Instance;
            
        }

        private void Start()
        {
            // 씬의 모든 유닛 찾기
            FindAllUnits();
        }

        // private bool playerAttacking = false;
        void Update()
        {

            // player 공격 로직
            for (int i = 0; i < allPlayerUnits.Length; i++)
            {
                if (allPlayerUnits[i] != null && allEnemyUnits.Length > 0)
                {
                    // playerAttacking = true;
                    if (allPlayerUnits[i].targetUnit == null)
                    {
                        foreach (Unit unitEnermy in allEnemyUnits)
                        {
                            if (unitEnermy != null && unitEnermy.playerUnit == false)
                            {
                                allPlayerUnits[i].targetUnit = unitEnermy;
                                EnermyContact(allPlayerUnits[i], unitEnermy);
                                break;
                            }
                        }
                    }
                }
            }

            // Enermy 공격 로직
            for (int i = 0; i < allEnemyUnits.Length; i++)
            {
                if (allEnemyUnits[i] != null && allPlayerUnits.Length > 0)
                {
                    // playerAttacking = false;
                    if (allEnemyUnits[i].targetUnit == null)
                    {
                        foreach (Unit unitPlayer in allPlayerUnits)
                        {
                            if (unitPlayer != null && unitPlayer.playerUnit)
                            {
                                allEnemyUnits[i].targetUnit = unitPlayer;
                                EnermyContact(allEnemyUnits[i], unitPlayer);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void AttackMode()
        {
            for (int i = 0; i < allPlayerUnits.Length; i++)
            {
                allPlayerUnits[i].SetBattleMode(UnitMode.Attack);
            }
        }

        public void DefenceMode()
        {
            for (int i = 0; i < allPlayerUnits.Length; i++)
            {
                allPlayerUnits[i].SetBattleMode(UnitMode.Defence);
            }

        }
        /// <summary>
        /// 씬에 존재하는 모든 유닛을 찾아서 플레이어/적군으로 분류
        /// </summary>
        private void FindAllUnits()
        {
            // 씬의 모든 Unit 컴포넌트 찾기
            Unit[] allUnits = FindObjectsOfType<Unit>();
            
            // playerUnit 필드로 구분
            System.Collections.Generic.List<Unit> playerList = new System.Collections.Generic.List<Unit>();
            System.Collections.Generic.List<Unit> enemyList = new System.Collections.Generic.List<Unit>();
            
            foreach (Unit unit in allUnits)
            {
                if (unit.playerUnit)
                {
                    playerList.Add(unit);
                }
                else
                {
                    enemyList.Add(unit);
                }
            }
            
            allPlayerUnits = playerList.ToArray();
            allEnemyUnits = enemyList.ToArray();
            
            Debug.Log($"플레이어 유닛 {allPlayerUnits.Length}개, 적 유닛 {allEnemyUnits.Length}개 발견");
        }


        /// <summary>
        /// 적과 아군 유닛이 접촉했을 때 전투를 시작하는 메서드
        /// </summary>
        /// <param name="attacker">공격의 주체자, 상호 각자가 공격자가 되야 한다.</param>
        /// <param name="defender"></param>
        public void EnermyContact(Unit attacker, Unit defender)
        {
            CombatAction action = new CombatAction
            {
                attacker = attacker,
                defender = defender,
                skillId = -1 // 기본 공격
            };

            combatSequence.QueueCombatAction(action);
            
        }

        /// <summary>
        /// 모든 아군 및 적군 유닛의 행동을 일시 정지하거나 재개합니다.
        /// </summary>
        /// <param name="pause">true일 경우 정지, false일 경우 재개</param>
        public void PauseAllUnits(bool pause)
        {
            PauseUI.SetActive(pause);
            
            if (allPlayerUnits != null)
            {
                foreach (Unit unit in allPlayerUnits)
                {
                    if (unit != null) unit.SetPause(pause);
                }
            }

            if (allEnemyUnits != null)
            {
                foreach (Unit unit in allEnemyUnits)
                {
                    if (unit != null) unit.SetPause(pause);
                }
            }
        }
    }
}
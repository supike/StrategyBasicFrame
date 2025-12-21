using System;
using UnityEngine;

namespace Combat
{
    public class CombatManager : MonoBehaviour
    {
        static public CombatManager Instance { get; private set; }
        private CombatSequence combatSequence;
        private CombatEventSystem combatEventSystem;
        [SerializeField]private Unit playerUnit;
        [SerializeField]private Unit[] enermyUnits;
        
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
            
            // 씬의 모든 유닛 찾기
            FindAllUnits();
        }

        private void Start()
        {
            // // 예시: 첫 번째 플레이어 유닛과 첫 번째 적 유닛 간 전투
            // if (allPlayerUnits.Length > 0 && allEnemyUnits.Length > 0)
            // {
            //     EnermyContact(allPlayerUnits[0], allEnemyUnits[0]);
            //     EnermyContact(allEnemyUnits[0], allPlayerUnits[0]); 
            // }
        }
        public void StartCombat()
        {
            // 전투 시작 로직 구현
            Debug.Log("전투 시작!");
            // 예시: 첫 번째 플레이어 유닛과 첫 번째 적 유닛 간 전투
            if (allPlayerUnits.Length > 0 && allEnemyUnits.Length > 0)
            {
                EnermyContact(allPlayerUnits[0], allEnemyUnits[0]);
                EnermyContact(allEnemyUnits[0], allPlayerUnits[0]); 
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
    }
}
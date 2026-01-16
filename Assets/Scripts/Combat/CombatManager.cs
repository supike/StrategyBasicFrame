using UnityEngine;
using Core;

namespace Combat
{
    /// <summary>
    /// 전투 상황을 관리하는 매니저 클래스
    /// GameManager의 UnitManager를 통해 유닛들을 관리합니다.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }
        
        private CombatSequence combatSequence;
        private UnitManager unitManager;
        private bool isPaused;

        [SerializeField] private GameObject pauseUI;

        public Unit[] GetAllPlayerUnits() => unitManager.GetPlayerUnits().ToArray();
        public Unit[] GetAllEnemyUnits() => unitManager.GetEnemyUnits().ToArray();
        
        #region 유니티 기본 함수
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
        }

        private void Start()
        {
            // GameManager에서 UnitManager 참조 가져오기
            unitManager = GameManager.Instance.UnitManager;
        }

        private void Update()
        {
            // ESC 키로 일시 정지 토글
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isPaused = !isPaused;
                PauseAllUnits(isPaused);
            }

            // 플레이어 유닛의 공격 로직
            ProcessPlayerAttack();

            // 적 유닛의 공격 로직
            ProcessEnemyAttack();
        }
        #endregion

        /// <summary>
        /// 전투를 초기화합니다. GameManager 또는 SceneManager에서 호출합니다.
        /// </summary>
        public void InitializeCombat()
        {
            Debug.Log($"[CombatManager] 전투 초기화 - 플레이어 유닛 {unitManager.GetPlayerUnits().Count}개, 적 유닛 {unitManager.GetEnemyUnits().Count}개");
        }

        /// <summary>
        /// 플레이어 유닛의 공격 로직 처리
        /// </summary>
        private void ProcessPlayerAttack()
        {
            if (unitManager == null || unitManager.GetEnemyUnits().Count == 0)
                return;

            foreach (Unit playerUnit in unitManager.GetPlayerUnits())
            {
                if (playerUnit != null && playerUnit.targetUnit == null)
                {
                    Unit target = unitManager.GetNearestEnemyUnit(playerUnit);
                    if (target != null)
                    {
                        playerUnit.targetUnit = target;
                        EnermyContact(playerUnit, target);
                    }
                }
            }
        }

        /// <summary>
        /// 적 유닛의 공격 로직 처리
        /// </summary>
        private void ProcessEnemyAttack()
        {
            if (unitManager == null || unitManager.GetPlayerUnits().Count == 0)
                return;

            foreach (Unit enemyUnit in unitManager.GetEnemyUnits())
            {
                if (enemyUnit != null && enemyUnit.targetUnit == null)
                {
                    Unit target = unitManager.GetNearestPlayerUnit(enemyUnit);
                    if (target != null)
                    {
                        enemyUnit.targetUnit = target;
                        EnermyContact(enemyUnit, target);
                    }
                }
            }
        }

        public void AttackMode()
        {
            unitManager.ForEachPlayerUnit(unit => unit.SetBattleMode(UnitMode.Attack));
        }

        public void DefenceMode()
        {
            unitManager.ForEachPlayerUnit(unit => unit.SetBattleMode(UnitMode.Defence));
        }

        
        /// <summary>
        /// 적과 아군 유닛이 접촉했을 때 전투를 시작하는 메서드
        /// </summary>
        /// <param name="attacker">공격의 주체자, 상호 각자가 공격자가 되야 한다.</param>
        /// <param name="defender">방어자</param>
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
            isPaused = pause;
            if (pauseUI != null)
            {
                pauseUI.SetActive(pause);
            }
            
            unitManager.ForEachAllUnits(unit => unit.SetPause(pause));
        }
    }
}
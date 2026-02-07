using UnityEngine;
using System.Collections.Generic;
using Combat;

namespace Core
{
    /// <summary>
    /// 게임 전역 상태를 관리하는 매니저 클래스
    /// 씬 전환 후에도 유지되어야 할 데이터를 보관합니다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private float productionTimer;
        private float turnTimer;
        private float productionTime = 1f; // 5초당 1자원

        public static GameManager Instance { get; private set; }

        // 유닛 관리자
        public UnitManager UnitManager { get; private set; }

        #region 유니티 기본 함수
        void Awake()
        {
            // VSync 비활성화 (targetFrameRate가 우선되도록)
            QualitySettings.vSyncCount = 0;

            // FPS를 60으로 제한
            Application.targetFrameRate = 60;

            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // UnitManager 초기화
            UnitManager = new UnitManager();
        }
        void Start()
        {
            productionTimer = 0;
            // 씬에 있는 Unit 컴포넌트를 찾아 플레이어 유닛 리스트로 설정
            var allUnits = FindObjectsOfType<Unit>();
            var playerUnits = new List<Unit>();
            var enemyUnits = new List<Unit>();

            // 게임오브젝트에 `Player` 태그를 사용 중이라면 이를 기준으로 필터링
            foreach (var u in allUnits)
            {
                // if (u.gameObject.CompareTag("Player"))
                if (u.playerUnit)
                {
                    playerUnits.Add(u);
                }
                else
                {
                    enemyUnits.Add(u);
                }
            }

            UnitManager.SetPlayerUnits(playerUnits);
            UnitManager.SetEnemyUnits(enemyUnits);
            // CombatManager.Instance.ProcessPlayerAttack();
        }

        // Update is called once per frame
        void Update()
        {
            // 실행을 하는 로직.
            // if (TurnManager.Instance.CurrentTurn == TurnManager.Turn.Action)
            // {
            //     // Handle resource production during the Action turn
            //     LiveTimeProduceBaseResource();
            //     LiveTimeTurnTick();
            // }
        }
        #endregion

        #region 유닛 관리 (UnitManager Wrapper)
        /// <summary>
        /// 플레이어 유닛 리스트를 설정합니다 (씬 시작 시 호출)
        /// </summary>
        public void SetPlayerUnits(List<Unit> units) => UnitManager.SetPlayerUnits(units);

        /// <summary>
        /// 적 유닛 리스트를 설정합니다 (전투 시작 시 호출)
        /// </summary>
        public void SetEnemyUnits(List<Unit> units) => UnitManager.SetEnemyUnits(units);

        /// <summary>
        /// 플레이어 유닛 리스트를 반환합니다
        /// </summary>
        public List<Unit> GetPlayerUnits() => UnitManager.GetPlayerUnits();

        /// <summary>
        /// 적 유닛 리스트를 반환합니다
        /// </summary>
        public List<Unit> GetEnemyUnits() => UnitManager.GetEnemyUnits();

        /// <summary>
        /// 전투 종료 후 적 유닛 리스트를 초기화합니다
        /// </summary>
        public void ClearEnemyUnits() => UnitManager.ClearEnemyUnits();
        #endregion


        void LiveTimeTurnTick()
        {
            // Handle resource production over turn time
            turnTimer += Time.deltaTime;

            if (turnTimer >= 1f)      //1초에 한시간씩 지남
            {
                turnTimer = 0f;
                // Halfway through the turn
                TurnManager.Instance.AddTurnHour();
            }
        }
        void LiveTimeProduceBaseResource()
        {
            // Handle resource production over time
            productionTimer += Time.deltaTime;

            if (productionTimer >= productionTime)
            {
                productionTimer = 0;
                ProduceBaseResource();
            }
        }
        void ProduceBaseResource()
        {
            for (int i = 0; i < (int)ResourceType.Count; i++)
            {
                ResourceManager.Instance.AddResource((ResourceType)i, 1);
            }
        }

    }

}
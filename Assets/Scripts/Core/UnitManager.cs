using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    /// <summary>
    /// 게임 전역의 모든 유닛을 관리하는 클래스
    /// GameManager의 유닛 관리 책임을 분담합니다.
    /// 
    /// 책임:
    /// - 유닛 조회 및 필터링
    /// - 거리 기반 검색
    /// - 유닛 상태 쿼리
    /// - 일괄 처리 작업
    /// </summary>
    public class UnitManager
    {
        private List<Unit> playerUnits;
        private List<Unit> enemyUnits;

        public UnitManager()
        {
            playerUnits = new List<Unit>();
            enemyUnits = new List<Unit>();
        }

        #region 유닛 설정
        /// <summary>
        /// 플레이어 유닛 리스트를 설정합니다
        /// </summary>
        public void SetPlayerUnits(List<Unit> units)
        {
            playerUnits = new List<Unit>(units);
            Debug.Log($"[UnitManager] 플레이어 유닛 {playerUnits.Count}개 설정");
        }

        /// <summary>
        /// 적 유닛 리스트를 설정합니다
        /// </summary>
        public void SetEnemyUnits(List<Unit> units)
        {
            enemyUnits = new List<Unit>(units);
            Debug.Log($"[UnitManager] 적 유닛 {enemyUnits.Count}개 설정");
        }

        /// <summary>
        /// 전투 종료 후 적 유닛을 초기화합니다
        /// </summary>
        public void ClearEnemyUnits()
        {
            enemyUnits.Clear();
            Debug.Log("[UnitManager] 적 유닛 초기화");
        }
        #endregion

        #region 유닛 조회
        /// <summary>
        /// 플레이어 유닛 리스트 반환
        /// </summary>
        public List<Unit> GetPlayerUnits() => new List<Unit>(playerUnits);

        /// <summary>
        /// 적 유닛 리스트 반환
        /// </summary>
        public List<Unit> GetEnemyUnits() => new List<Unit>(enemyUnits);

        /// <summary>
        /// 살아있는 플레이어 유닛만 반환
        /// </summary>
        public List<Unit> GetAlivePlayerUnits() => playerUnits.Where(u => u != null && u.IsAlive).ToList();

        /// <summary>
        /// 살아있는 적 유닛만 반환
        /// </summary>
        public List<Unit> GetAliveEnemyUnits() => enemyUnits.Where(u => u != null && u.IsAlive).ToList();

        /// <summary>
        /// 특정 유닛에서 가장 가까운 적 유닛을 찾음
        /// </summary>
        public Unit GetNearestEnemyUnit(Unit sourceUnit)
        {
            if (sourceUnit == null || !sourceUnit.playerUnit)
                return null;

            Unit nearest = null;
            float minDistance = float.MaxValue;

            foreach (Unit enemy in GetAliveEnemyUnits())
            {
                float distance = Vector3.Distance(sourceUnit.transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 특정 유닛에서 가장 가까운 플레이어 유닛을 찾음
        /// </summary>
        public Unit GetNearestPlayerUnit(Unit sourceUnit)
        {
            if (sourceUnit == null || sourceUnit.playerUnit)
                return null;

            Unit nearest = null;
            float minDistance = float.MaxValue;

            foreach (Unit player in GetAlivePlayerUnits())
            {
                float distance = Vector3.Distance(sourceUnit.transform.position, player.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = player;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 특정 위치에서 가장 가까운 유닛을 찾음 (플레이어 또는 적)
        /// </summary>
        public Unit GetNearestUnit(Vector3 position, bool findPlayer = true)
        {
            List<Unit> targetList = findPlayer ? GetAlivePlayerUnits() : GetAliveEnemyUnits();
            Unit nearest = null;
            float minDistance = float.MaxValue;

            foreach (Unit unit in targetList)
            {
                float distance = Vector3.Distance(position, unit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = unit;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 특정 범위 내의 모든 유닛을 찾음
        /// </summary>
        public List<Unit> GetUnitsInRange(Vector3 position, float range, bool getPlayers = true)
        {
            List<Unit> result = new List<Unit>();
            List<Unit> targetList = getPlayers ? GetAlivePlayerUnits() : GetAliveEnemyUnits();

            foreach (Unit unit in targetList)
            {
                float distance = Vector3.Distance(position, unit.transform.position);
                if (distance <= range)
                {
                    result.Add(unit);
                }
            }

            return result;
        }
        #endregion

        #region 유닛 상태 쿼리
        /// <summary>
        /// 플레이어 유닛이 모두 사망했는지 확인
        /// </summary>
        public bool AreAllPlayerUnitsDead() => GetAlivePlayerUnits().Count == 0;

        /// <summary>
        /// 적 유닛이 모두 사망했는지 확인
        /// </summary>
        public bool AreAllEnemyUnitsDead() => GetAliveEnemyUnits().Count == 0;

        /// <summary>
        /// 전투 중인지 확인 (적 유닛이 있는지)
        /// </summary>
        public bool IsInBattle() => enemyUnits.Count > 0;

        /// <summary>
        /// 생존한 유닛 개수
        /// </summary>
        public int GetAliveUnitCount(bool getPlayers = true)
        {
            return getPlayers ? GetAlivePlayerUnits().Count : GetAliveEnemyUnits().Count;
        }
        #endregion

        #region 유닛 일괄 처리
        /// <summary>
        /// 모든 플레이어 유닛에 콜백 실행
        /// </summary>
        public void ForEachPlayerUnit(System.Action<Unit> action)
        {
            foreach (Unit unit in playerUnits)
            {
                if (unit != null) action?.Invoke(unit);
            }
        }

        /// <summary>
        /// 모든 적 유닛에 콜백 실행
        /// </summary>
        public void ForEachEnemyUnit(System.Action<Unit> action)
        {
            foreach (Unit unit in enemyUnits)
            {
                if (unit != null) action?.Invoke(unit);
            }
        }

        /// <summary>
        /// 모든 유닛에 콜백 실행
        /// </summary>
        public void ForEachAllUnits(System.Action<Unit> action)
        {
            ForEachPlayerUnit(action);
            ForEachEnemyUnit(action);
        }
        #endregion

        #region 디버그
        /// <summary>
        /// 현재 유닛 상태 로그 출력
        /// </summary>
        public void LogUnitStatus()
        {
            Debug.Log($"=== Unit Status ===");
            Debug.Log($"플레이어 유닛: {GetAlivePlayerUnits().Count}/{playerUnits.Count} 생존");
            Debug.Log($"적 유닛: {GetAliveEnemyUnits().Count}/{enemyUnits.Count} 생존");
            Debug.Log($"전투 중: {(IsInBattle() ? "Yes" : "No")}");
        }
        #endregion
    }
}


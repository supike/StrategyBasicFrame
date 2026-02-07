using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum UnitMode
{
    Normal,
    Attack,
    Defence,
    Retreat,
}

[System.Serializable]
public class UnitStats
{


    [Header("모드")]
    public UnitMode unitMode = UnitMode.Normal;


    [Header("기본 스탯")]
    public int maxHealth;
    public int maxStamina;
    public HealthCondition healthCond;
    public EmotionCondition emotionCond;


    [Header("공격 관련")]
    public int baseAttack;          // 기본 공격력
    public int attackVariation = 2; // 데미지 랜덤성 ±2
    public float attackSpeed = 2.5f; // 공격 속도 (초)

    [Header("방어 관련")]
    public int physicalDefense;     // 물리 방어
    public int magicalDefense;      // 마법 방어
    public int evasion;             // 회피율 (0-100)

    [Header("이동 관련")]
    public int movement;            // 이동력
    public int initiative;          // 선공권 (높을수록 먼저)

    [Header("특수 스탯")]
    public int morale = 100;        // 사기 (0-100)
    public int experience = 0;      // 경험치
    public int level = 1;           // 레벨

}

// todo : Unit 클래스 리팩토링 필요
public class Unit : MonoBehaviour
{
    #region 기본 데이터
    [SerializeField] public bool playerUnit = false;
    [SerializeField] public CharacterData unitData;
    [SerializeField] public UnitStats stats;
    [SerializeField] private Tilemap tilemap;

    public Unit targetUnit { get; set; }
    public string UnitName => unitData.characterName;
    public int CurrentHealth { get; set; }
    public float CurrentStamina { get; set; }
    public float CurrentBalance { get; set; }
    public int CurrentMorale { get; private set; }
    public int MaxHealth => unitData.health;
    public int MaxStamina => unitData.stamina;
    public int MaxBalance => unitData.balance;
    public int AttackPower => unitData.strength;
    // public int MovementRange => unitData.movementRange;
    public bool IsAlive => CurrentHealth > 0;
    #endregion

    #region 타일 및 이동

    [SerializeField] public TileCustomWithEvent CurrentTile;
    private bool hasMovedThisTurn;
    private bool hasAttackedThisTurn;
    public bool isMoving;
    #endregion

    #region 컴포넌트
    private Animator animator;
    public UnitUI unitUI;
    #endregion

    #region 상태
    [SerializeField] private int direction = 1;
    private bool readyForAttack;
    private bool isPaused;
    public bool isAttacking;
    public bool IsStunned;
    public bool IsSlowed;
    public bool doingCombo = false;
    #endregion

    #region 이벤트
    public System.Action<Unit> OnReadyForAttack;
    #endregion


    private void Awake()
    {
        animator = GetComponent<Animator>();
        unitUI = GetComponent<UnitUI>();

        if (unitUI == null)
        {
            Debug.LogWarning($"[Unit] {gameObject.name}: UnitUI 컴포넌트가 없습니다. UI 기능이 제한될 수 있습니다.");
        }

        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        CurrentBalance = MaxBalance;
    }

    void Start()
    {
        // UI 초기화
        if (unitUI != null)
        {
            unitUI.Initialize(UnitName, unitData.iconImage);
            unitUI.UpdateHealthUI(CurrentHealth, MaxHealth);
            unitUI.SetBattleModeIcon(UnitMode.Normal);
        }

        SetBattleMode(UnitMode.Normal);

        // 타일 초기화
        Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
        TileBase curTile = Instantiate(tilemap.GetTile<TileBase>(cellPosition));

        if (curTile is TileCustomWithEvent tileWithEvent)
        {
            Debug.Log("TileCustomWithEvent found");
            tileWithEvent.Initialize(cellPosition.x, cellPosition.y, false);
            Initialize(tileWithEvent);
        }
        else
        {
            // Debug.LogWarning($"[Unit Start] TileCustomWithEvent NOT found at {cellPosition}");
        }

        // 셀의 중심 월드 좌표를 가져옴
        Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
        // Debug.Log($"[Unit Start] centerPosition: {centerPosition}");

        StartCoroutine(MovePlayer(centerPosition));

    }
    public void Update()
    {
        // 애니메이션 상태에 따른 이동 플래그 설정
        if (animator != null)
        {
            // isMoving = animator.GetCurrentAnimatorStateInfo(0).IsName("Walk");
        }
        // 방향에 따라 객체 뒤집기
        SetDirection(direction);


        // 1초에 stamina와 balance를 서서히 회복한다.
        if (!isAttacking && !isMoving)
        {
            float recoveryRate = Time.deltaTime / 2; // 초당 회복량

            // Stamina 회복
            if (CurrentStamina < MaxStamina)
            {
                CurrentStamina = Mathf.Min((float)CurrentStamina + recoveryRate, (float)MaxStamina);
                unitUI?.UpdateStaminaUI(CurrentStamina / MaxStamina);
            }

            // Balance 회복
            if (CurrentBalance < MaxBalance)
            {
                CurrentBalance = Mathf.Min((float)CurrentBalance + recoveryRate, (float)MaxBalance);
                unitUI?.UpdateBalanceUI(CurrentBalance / MaxBalance);
            }
        }
    }

    public void SetPause(bool pause)
    {
        isPaused = pause;
        if (animator != null)
        {
            animator.speed = isPaused ? 0f : 1f;
        }
    }

    /// <summary>
    /// 공격 준비. 쿨타임 UI 초기화 및 공격 속도에 따른 플래그 설정
    /// </summary>
    public IEnumerator PrepareForAction()
    {
        // 타겟 유닛이 없으면 즉시 정지
        if (targetUnit == null || !targetUnit.IsAlive)
        {
            Debug.LogWarning($"[{UnitName}] targetUnit is null or not alive. PrepareForAction stopped.");
            yield break;
        }

        unitUI?.UpdateCoolTimeUI(0f);

        float elapsedTime = 0f;
        while (elapsedTime < stats.attackSpeed)
        {
            // 진행 중 타겟이 사라지면 즉시 정지
            if (targetUnit == null || !IsAlive)
            {
                Debug.LogWarning($"[{UnitName}] targetUnit disappeared during PrepareForAction. Stopping.");
                unitUI?.UpdateCoolTimeUI(0f);
                yield break;
            }

            if (isPaused)
            {
                yield return null;
                continue;
            }

            unitUI?.UpdateCoolTimeUI(elapsedTime / stats.attackSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        unitUI?.UpdateCoolTimeUI(1f);
        OnReadyForAttack?.Invoke(this);
    }

    // public IEnumerator RunAttackCooldown()
    // {
    // }
    public void Initialize(TileCustomWithEvent startTile)
    {
        // GridManager의 실제 타일 참조 사용 (Instantiate 사용 금지)
        CurrentTile = GridManager.Instance.GetTileAtCellPosition(new Vector3Int(startTile.X, startTile.Y, 0));
        CurrentTile.SetOccupied(this);

        Debug.Log("Tile Type is: " + CurrentTile.TileType);
    }

    public void ResetTurnActions()
    {
        hasMovedThisTurn = false;
        hasAttackedThisTurn = false;
    }

    public bool CanMove() => !hasMovedThisTurn && IsAlive;
    public bool CanAttack() => !hasAttackedThisTurn && IsAlive;

    public List<TileCustomWithEvent> GetMovableTiles()
    {
        List<TileCustomWithEvent> movableTiles = new List<TileCustomWithEvent>();
        Queue<TileCustomWithEvent> queue = new Queue<TileCustomWithEvent>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(CurrentTile);
        visited.Add(CurrentTile.GridPosition);
        CurrentTile.GCost = 0;

        while (queue.Count > 0)
        {
            TileCustomWithEvent current = queue.Dequeue();
            Vector3Int currentCell = new Vector3Int(current.X, current.Y, 0);

            foreach (TileCustomWithEvent neighbor in GridManager.Instance.GetNeighbors(currentCell))
            {
                // if (visited.Contains(neighbor.GridPosition)) continue;

                if (neighbor.OccupyingUnit == null)
                {
                    movableTiles.Add(neighbor);

                }

                // 점유 여부 확인
                // if (neighbor.OccupyingUnit != null) continue;
                //
                // int newCost = current.GCost + 1;
                //
                // if (newCost <= MovementRange)
                // {
                //     // BFS 탐색용 복사본 생성
                //     TileCustomWithEvent neighborCopy = Instantiate(neighbor);
                //     neighborCopy.Initialize(neighbor.X, neighbor.Y, true);
                //     neighborCopy.GCost = newCost;
                //
                //     movableTiles.Add(neighborCopy);
                //     queue.Enqueue(neighborCopy);
                //     visited.Add(neighborCopy.GridPosition);
                // }
            }
        }

        return movableTiles;
    }

    #region 유닛 행동
    public void Retreat()
    {
        // 후퇴 로직 구현
        Debug.Log($"{UnitName} is retreating!");
        PlayAnimation("Retreat");
        // 추가적인 후퇴 행동 구현 가능
        StartCoroutine(MoveTo(new TileCustomWithEvent(-5, 0, true)));
    }
    /// <summary>
    /// 유닛을 타겟 타일로 이동시키는 코루틴
    /// 타일 간 이동 애니메이션 포함
    /// </summary>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public IEnumerator MoveTo(TileCustomWithEvent targetTile)
    {
        if (!CanMove()) yield break;

        // 타겟 타일 점유 확인 (이중 체크)
        TileCustomWithEvent targetGridTile = GridManager.Instance.GetTileAtCellPosition(new Vector3Int(targetTile.X, targetTile.Y, 0));
        if (targetGridTile.OccupyingUnit != null && targetGridTile.OccupyingUnit != this)
        {
            Debug.LogWarning("Target tile is occupied!");
            yield break;
        }

        // 이전 위치 점유 해제
        GridManager.Instance.GetTileAtCellPosition(new Vector3Int(CurrentTile.X, CurrentTile.Y, 0)).OccupyingUnit = null;
        CurrentTile.ClearOccupied();

        // 새로운 위치 점유 설정 (이동 시작 전에 미리 점유!)
        GridManager.Instance.GetTileAtCellPosition(new Vector3Int(targetTile.X, targetTile.Y, 0)).OccupyingUnit = this;

        Vector3 centerPosition = tilemap.GetCellCenterWorld(new Vector3Int(targetTile.GridPosition.x, targetTile.GridPosition.y, 0));

        // 이동 방향 설정
        if (targetTile.X > CurrentTile.X)
        {
            SetDirection(1); // 오른쪽
        }
        else if (targetTile.X < CurrentTile.X)
        {
            SetDirection(-1); // 왼쪽
        }

        // 현재 타일 정보 업데이트 - GridManager의 실제 타일을 참조
        CurrentTile = GridManager.Instance.GetTileAtCellPosition(new Vector3Int(targetTile.X, targetTile.Y, 0));

        // 이동 애니메이션
        yield return StartCoroutine(MovePlayer(centerPosition));

        // hasMovedThisTurn = true;
    }
    #endregion


    public void Attack(Unit target)
    {
        if (!CanAttack()) return;

        int damage = AttackPower;
        target.TakeDamage(damage);
        hasAttackedThisTurn = true;
    }

    public bool TakeBalance(int damage)
    {
        CurrentBalance -= damage;
        CurrentBalance = Mathf.Max(0, CurrentBalance);

        // UI 업데이트
        unitUI?.UpdateBalanceUI(CurrentBalance / (float)MaxBalance);

        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, damage.ToString(), DamageType.Poison);
        // PlayAnimation("Hit");

        return IsAlive;
    }

    public bool TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);

        // UI 업데이트
        unitUI?.UpdateHealthUI(CurrentHealth, MaxHealth);

        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, damage.ToString());
        PlayAnimation("Hit");
        if (!IsAlive)
        {
            Die();
        }

        return IsAlive;
    }

    public void Dodge()
    {
        // DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, "Dodge!");
        PlayAnimation("Dodge");
        Vector3Int aTile;

        TileCustomWithEvent tc = GridManager.Instance.GetTileAtCellPosition(new Vector3Int(CurrentTile.X, CurrentTile.Y, 0));
        Debug.Log($"Tile initialized at ({tc.X}, {tc.Y})");
        CurrentTile.X += direction * -1;
        tc = GridManager.Instance.GetTileAtCellPosition(new Vector3Int(CurrentTile.X, CurrentTile.Y, 0));
        Debug.Log($"Tile initialized at ({tc.X}, {tc.Y})");
        tc = GridManager.Instance.GetTileAtCellPosition(new Vector3Int(CurrentTile.X - 1, CurrentTile.Y, 0));

        StartCoroutine(MoveTo(CurrentTile));


        Debug.LogWarning("Target tile is occupied!");
        // MovePlayer(new Vector3(CurrentTile.GridPosition.x - 1, CurrentTile.GridPosition.y), 1);
    }


    public void DecreaseBalance(int value = 1)
    {
        CurrentBalance -= value;
        CurrentBalance = Mathf.Clamp(CurrentBalance, 0, MaxBalance);
        unitUI?.UpdateBalanceUI(CurrentBalance / (float)MaxBalance);
    }

    public void IncreaseBalance(int value)
    {
        CurrentBalance++;
        CurrentBalance = Mathf.Clamp(CurrentBalance, 0, MaxBalance);
        unitUI?.UpdateBalanceUI(CurrentBalance / (float)MaxBalance);
    }

    public void DecreaseStamina()
    {
        if (doingCombo)
        {
            // CurrentStamina-=4;
        }
        CurrentStamina -= 4;
        unitUI?.UpdateStaminaUI(CurrentStamina / (float)MaxStamina);
        doingCombo = true;
    }
    public void RecoverStamina()
    {
        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, "Recover");
        CurrentStamina += 3;
        unitUI?.UpdateStaminaUI(CurrentStamina / (float)MaxStamina);
        doingCombo = false;
    }
    void Die()
    {
        CurrentTile.ClearOccupied();
        // GridManager에서도 제거
        GridManager.Instance.GetTileAtCellPosition(new Vector3Int(CurrentTile.X, CurrentTile.Y, 0)).OccupyingUnit = null;

        // Destroy(gameObject);
    }

    // 적 AI (간단한 예시)
    public IEnumerator ExecuteAI()
    {
        // 가장 가까운 플레이어 유닛 찾기
        Unit target = FindNearestPlayerUnit();

        if (target != null)
        {
            // 공격 범위 내면 공격
            if (IsInAttackRange(target))
            {
                Attack(target);
            }
            else
            {
                // 이동
                List<TileCustomWithEvent> movableTiles = GetMovableTiles();
                TileCustomWithEvent closestTile = GetClosestTileToTarget(movableTiles, target.CurrentTile);

                if (closestTile != null)
                {
                    yield return StartCoroutine(MoveTo(closestTile));
                }
            }
        }

        yield return new WaitForSeconds(1f);
    }

    Unit FindNearestPlayerUnit()
    {
        Unit nearestUnit = null;
        int minDistance = int.MaxValue;

        // 씬의 모든 Unit 찾기
        Unit[] allUnits = FindObjectsOfType<Unit>();

        foreach (Unit unit in allUnits)
        {
            // 플레이어 유닛이고, 살아있으며, 자신이 아닌 경우만 고려
            if (unit.playerUnit && unit.IsAlive && unit != this)
            {
                // 맨해튼 거리 계산
                int distance = Mathf.Abs(CurrentTile.X - unit.CurrentTile.X) +
                              Mathf.Abs(CurrentTile.Y - unit.CurrentTile.Y);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestUnit = unit;
                }
            }
        }

        return nearestUnit;
    }

    bool IsInAttackRange(Unit target)
    {
        int distance = Mathf.Abs(CurrentTile.X - target.CurrentTile.X) +
                      Mathf.Abs(CurrentTile.Y - target.CurrentTile.Y);
        return distance == 1; // 인접 공격만
    }

    TileCustomWithEvent GetClosestTileToTarget(List<TileCustomWithEvent> tiles, TileCustomWithEvent targetTile)
    {
        // 구현 생략
        return null;
    }

    /// <summary>
    /// 유닛을 지정된 방향으로 부드럽게 이동시키는 코루틴
    /// 좌표는 타일맵의 중심 좌표여야 함
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    IEnumerator MovePlayer(Vector3 direction, float _forceSpeed = 0)
    {
        isMoving = true;

        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = direction;
        targetPosition.y = targetPosition.y + 1.25f; // Adjust for unit height

        float distance = Vector3.Distance(startPosition, targetPosition);

        float speed = _forceSpeed != 0 ? _forceSpeed : stats.movement;
        float moveDuration = speed > 0 ? distance / speed : 1f; // 기본값 설정

        while (elapsedTime < moveDuration)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsedTime / moveDuration  // 0 -> 1로 선형 증가
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;

        yield return null;

    }

    public void PlayAnimation(string attack)
    {
        // animator.Play(attack);
        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, attack);
        animator.SetTrigger(attack);

    }

    public void SetDirection(int dir)
    {
        direction = dir >= 0 ? 1 : -1;

        // 스프라이트 방향 변경
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;

        // UI 방향 변경
        unitUI?.SetUIDirection(direction);
    }

    public void SetBattleMode(UnitMode unitBattleMode)
    {
        stats.unitMode = unitBattleMode;
        unitUI?.SetBattleModeIcon(unitBattleMode);
    }

    public bool IsHalfHealth()
    {
        if (CurrentHealth <= MaxHealth / 2)
        {
            return true;
        }
        return false;
    }
}

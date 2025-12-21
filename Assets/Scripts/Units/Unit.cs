using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[System.Serializable]
public class UnitStats
{
    [Header("기본 스탯")]
    public int currentHealth;
    public int maxHealth;
    
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
public class Unit : MonoBehaviour
{
    [SerializeField] public  bool playerUnit = false;
    [SerializeField] public  UnitDataSO unitData;
    [SerializeField] public UnitStats stats;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Animator animator;
    [SerializeField] private Image attackCoolTimeImage;
    [SerializeField] private TMPro.TextMeshProUGUI healthText;
    [SerializeField] private Image healthSlider;
    [SerializeField] private TMPro.TextMeshProUGUI name;
    public Unit targetUnit{ get; set; }
    public string UnitName => unitData.unitName;
    public int CurrentHealth { get; private set; }  //체력
    public int CurrentStamina { get; private set; } //지구력: 행동 후 감소, 휴식 시 회복
    public int CurrentMorale { get; private set; }  //사기: 높으면 공격적, 낮으면 방어적
    public int MaxHealth => unitData.maxHealth;
    public int AttackPower => unitData.attack;
    public int MovementRange => unitData.movementRange;
    public bool IsAlive => CurrentHealth > 0;
    
    public TileCustomWithEvent CurrentTile { get; private set; }
    
    private bool hasMovedThisTurn = false;
    private bool hasAttackedThisTurn = false;
    private bool isMoving;
    [SerializeField] private int direction;     // 1: 오른쪽, -1: 왼쪽
    
    
    [SerializeField] private float moveSpeed = 5f; // 이동 속도 (units per second)
    public System.Action<Unit> OnReadyForAttack;

    private bool readyForAttack = false;
    private GameObject charStatusUI ;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        CurrentHealth = MaxHealth;
        charStatusUI = this.transform.Find("CharStatusUI").gameObject;
        
        // Combat.CombatEventSystem.Instance.OnCombatEvent += HandleCombatEvent;
    }

    void Start()
    {
        // 유닛 초기화
        name.text = UnitName;
        healthText.text = $"{CurrentHealth} / {MaxHealth}";
        healthSlider.fillAmount = (float)CurrentHealth / MaxHealth;
        /////////////////////ToDo Test, Delete Later//////////////////
        /// 로직 테스트용 목적으로 구현하지만 나중에는 구조적으로 분리하자.
        Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
        Debug.Log($"[Unit Start] cellPosition: {cellPosition}");
        
        TileBase curTile = tilemap.GetTile<TileBase>(cellPosition);
        Debug.Log($"[Unit Start] curTile type: {curTile?.GetType().Name}, curTile: {curTile}");
        
        if (curTile is TileCustomWithEvent)
        {
            Debug.Log("TileCustomWithEvent found");
            TileCustomWithEvent tileWithEvent = curTile as TileCustomWithEvent;
            
            // Initialize 전 좌표 출력
            Debug.Log($"[Before Initialize] Tile coords: ({tileWithEvent.X}, {tileWithEvent.Y})");
            
            // 타일의 좌표를 실제 셀 좌표로 초기화
            tileWithEvent.Initialize(cellPosition.x, cellPosition.y, true);
            
            // Initialize 후 좌표 출력
            Debug.Log($"[After Initialize] cellPosition: ({cellPosition.x}, {cellPosition.y}) -> Tile coords: ({tileWithEvent.X}, {tileWithEvent.Y})");
            
            // 검증: 좌표가 일치하는지 확인
            if (tileWithEvent.X == cellPosition.x && tileWithEvent.Y == cellPosition.y)
            {
                Debug.Log($"✓ 좌표 동기화 성공!");
            }
            else
            {
                Debug.LogWarning($"✗ 좌표 불일치! cellPosition: ({cellPosition.x}, {cellPosition.y}), Tile: ({tileWithEvent.X}, {tileWithEvent.Y})");
            }
            
            Initialize(tileWithEvent);
        }
        else
        {
            Debug.LogWarning($"[Unit Start] TileCustomWithEvent NOT found at {cellPosition}");
        }
        
        // 셀의 중심 월드 좌표를 가져옴
        Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
        Debug.Log($"[Unit Start] centerPosition: {centerPosition}");
 
        StartCoroutine(MovePlayer(centerPosition));

    }
    public void Update()
    {
        // 애니메이션 상태에 따른 이동 플래그 설정
        if (animator != null)
        {
            isMoving = animator.GetCurrentAnimatorStateInfo(0).IsName("Walk");
        }
        // 방향에 따라 객체 뒤집기
        SetDirection(direction);

        // 테스트용 이동 코드, 나중에 삭제 필요
        if (Input.GetKeyDown(KeyCode.RightArrow) && playerUnit)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
            Debug.Log("world to cell: "+cellPosition);
            cellPosition.x++;
            
            Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
            Debug.Log("centerPosition: "+centerPosition);
                
            StartCoroutine(MovePlayer(centerPosition));
            //StartCoroutine(MovePlayer(new Vector3(2, 0, 0)));
            // Debug.Log("world to cell: "+cellPosition);
            
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && playerUnit)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
            Debug.Log("world to cell: "+cellPosition);
            cellPosition.x--;
            Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
            Debug.Log("centerPosition: "+centerPosition);
            
            StartCoroutine(MovePlayer(centerPosition));
        }

        if (Input.GetMouseButtonDown(0) && playerUnit)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);
            Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
            //todo : 이동 가능 타일인지 체크 필요
            StartCoroutine(MoveTo(GridManager.Instance.GetTileAtCellPosition(cellPosition))); 
            // StartCoroutine(MovePlayer(centerPosition));
        }
    }
    
    /// <summary>
    /// 공격 준비. 쿨타임 UI 초기화 및 공격 속도에 따른 플래그 설정
    /// </summary>
    public IEnumerator PrepareForAttack()
    {
        attackCoolTimeImage.fillAmount = 0f;
        // readyForAttack = true;
        // yield return StartCoroutine(RunAttackCooldown());
        float elapsedTime = 0f;
        while (elapsedTime < stats.attackSpeed)
        {
            attackCoolTimeImage.fillAmount = elapsedTime / stats.attackSpeed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        attackCoolTimeImage.fillAmount = 1f;
        OnReadyForAttack?.Invoke(this);
    }

    // public IEnumerator RunAttackCooldown()
    // {
    // }
    public void Initialize(TileCustomWithEvent startTile)
    {
        CurrentTile = startTile;
        startTile.SetOccupied(this);
        Debug.Log("Tile Type is: " + CurrentTile.TileType);
        ;
        //transform.position = startTile.transform.position;
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
        HashSet<TileCustomWithEvent> visited = new HashSet<TileCustomWithEvent>();
        
        queue.Enqueue(CurrentTile);
        visited.Add(CurrentTile);
        CurrentTile.GCost = 0;
        
        while (queue.Count > 0)
        {
            TileCustomWithEvent current = queue.Dequeue();
            
            foreach (TileCustomWithEvent neighbor in GridManager.Instance.GetNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;
                
                int newCost = current.GCost + 1;
                
                if (newCost <= MovementRange)
                {
                    neighbor.GCost = newCost;
                    
                    if (neighbor.OccupyingUnit == null)
                    {
                        movableTiles.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                    
                    visited.Add(neighbor);
                }
            }
        }
        
        return movableTiles;
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
        
        List<TileCustomWithEvent> path = Pathfinding.Instance.FindPath(CurrentTile, targetTile);
        
        if (path != null && path.Count > 0)
        {
            CurrentTile.ClearOccupied();
            
            // path의 각 타일을 순차적으로 이동
            foreach (TileCustomWithEvent tile in path)
            {
                // 타일의 셀 좌표를 월드 좌표로 변환
                Vector3Int cellPosition = new Vector3Int(tile.X, tile.Y, 0);
                Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
                
                // 이동 방향 설정 (좌우 반전용)
                if (tile.X > CurrentTile.X)
                {
                    SetDirection(1); // 오른쪽
                }
                else if (tile.X < CurrentTile.X)
                {
                    SetDirection(-1); // 왼쪽
                }
                
                // 해당 타일로 이동 (코루틴 완료 대기)
                yield return StartCoroutine(MovePlayer(centerPosition));
                
                // 현재 타일 업데이트
                CurrentTile = tile;
            }
            
            // 최종 목적지 타일 점유 설정
            targetTile.SetOccupied(this);
            hasMovedThisTurn = true;
        }
    }
    
    public void Attack(Unit target)
    {
        if (!CanAttack()) return;
        
        int damage = AttackPower;
        target.TakeDamage(damage);
        hasAttackedThisTurn = true;
    }
    
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        healthText.text = $"{CurrentHealth} / {MaxHealth}";
        healthSlider.fillAmount = (float)CurrentHealth / MaxHealth;
        if (!IsAlive)
        {
            Die();
        }
        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, damage.ToString());
    }

    public void Dodge()
    {
        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, "Miss");
    }
    void Die()
    {
        CurrentTile.ClearOccupied();
        PlayAnimation("Die");
        Destroy(gameObject);
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
        // 구현 생략
        return null;
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
    IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;
        
        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = direction;
        targetPosition.y = targetPosition.y + 1.25f; // Adjust for unit height
        
        float distance = Vector3.Distance(startPosition, targetPosition);
        float moveDuration = distance / moveSpeed; // Calculate duration based on distance and speed
        
        while (elapsedTime < moveDuration)
        {
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
    }

    public void PlayAnimation(string attack)
    {
        animator.Play(attack);
    }

    public void SetDirection(int dir)
    {
        direction = dir >= 0 ? 1 : -1;

        // body가 애니메이터에 의해 스케일이 덮어쓰이지 않는 객체여야 함.
        Vector3 scale =this.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        this.transform.localScale = scale;
        
        Vector3 scaleui =charStatusUI.transform.localScale;
        scaleui .x = Mathf.Abs(scaleui.x) * direction;
        charStatusUI.transform.localScale = scaleui;
    }
}


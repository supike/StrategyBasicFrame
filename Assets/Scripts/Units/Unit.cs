using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    public bool isMoving = false;
    private bool isAttacking;
    [SerializeField] private int direction;     // 1: 오른쪽, -1: 왼쪽
    
    
    [SerializeField] private float moveSpeed = 5f; // 이동 속도 (units per second)
    public System.Action<Unit> OnReadyForAttack;

    private bool readyForAttack = false;
    private bool isPaused = false;
    private GameObject charStatusUI ;
    
    public GameObject playerUI;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        CurrentHealth = MaxHealth;
        charStatusUI = this.transform.Find("CharStatusUI").gameObject;

        // playerUI = GetComponentIndex();
        // Combat.CombatEventSystem.Instance.OnCombatEvent += HandleCombatEvent;
    }

    void Start()
    {
        // 유닛 초기화
        name.text = UnitName;
        healthText.text = $"{CurrentHealth} / {MaxHealth}";
        healthSlider.fillAmount = (float)CurrentHealth / MaxHealth;
        
        SpriteRenderer sr = playerUI.GetComponentInChildren<SpriteRenderer>();
        sr.sprite = unitData.icon;
        
        
        /////////////////////ToDo Test, Delete Later//////////////////
        /// 로직 테스트용 목적으로 구현하지만 나중에는 구조적으로 분리하자.
        Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
        Debug.Log($"[Unit Start] cellPosition: {cellPosition}");
        
        TileBase curTile = Instantiate(tilemap.GetTile<TileBase>(cellPosition)) ;
        Debug.Log($"[Unit Start] curTile type: {curTile?.GetType().Name}, curTile: {curTile}");
        
        if (curTile is TileCustomWithEvent)
        {
            Debug.Log("TileCustomWithEvent found");
            TileCustomWithEvent tileWithEvent = curTile as TileCustomWithEvent;
            tileWithEvent.Initialize(cellPosition.x, cellPosition.y, false);
            
            // Initialize 전 좌표 출력
            Debug.Log($"[Before Initialize] Tile coords: ({tileWithEvent.X}, {tileWithEvent.Y})");
            
            // 타일의 좌표를 실제 셀 좌표로 초기화
            //tileWithEvent.Initialize(cellPosition.x, cellPosition.y, true);
            
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
            // isMoving = animator.GetCurrentAnimatorStateInfo(0).IsName("Walk");
        }
        // 방향에 따라 객체 뒤집기
        SetDirection(direction);

        // 테스트용 이동 코드, 나중에 삭제 필요
        // if (Input.GetKeyDown(KeyCode.RightArrow) && playerUnit)
        // {
        //     Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
        //     Debug.Log("world to cell: "+cellPosition);
        //     cellPosition.x++;
        //     
        //     Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
        //     Debug.Log("centerPosition: "+centerPosition);
        //         
        //     StartCoroutine(MovePlayer(centerPosition));
        //     //StartCoroutine(MovePlayer(new Vector3(2, 0, 0)));
        //     // Debug.Log("world to cell: "+cellPosition);
        //     
        // }
        //
        // if (Input.GetKeyDown(KeyCode.LeftArrow) && playerUnit)
        // {
        //     Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
        //     Debug.Log("world to cell: "+cellPosition);
        //     cellPosition.x--;
        //     Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
        //     Debug.Log("centerPosition: "+centerPosition);
        //     
        //     StartCoroutine(MovePlayer(centerPosition));
        // }
        //
        // if (Input.GetMouseButtonDown(0) && playerUnit)
        // {
        //     Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);
        //     Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
        //     //todo : 이동 가능 타일인지 체크 필요
        //     StartCoroutine(MoveTo(GridManager.Instance.GetTileAtCellPosition(cellPosition))); 
        //     // StartCoroutine(MovePlayer(centerPosition));
        // }
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
    public IEnumerator PrepareForAttack()
    {
        attackCoolTimeImage.fillAmount = 0f;
        // readyForAttack = true;
        // yield return StartCoroutine(RunAttackCooldown());
        float elapsedTime = 0f;
        while (elapsedTime < stats.attackSpeed)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }
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
        CurrentTile = Instantiate(startTile);
        CurrentTile.Initialize(startTile.X, startTile.Y, true);

        CurrentTile.SetOccupied(this);
        
        // GridManager에 유닛 등록
        GridManager.Instance.GetTileAtCellPosition(new Vector3Int(startTile.X, startTile.Y, 0)).OccupyingUnit = this;
        
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

        // 현재 타일 정보 업데이트
        CurrentTile.Initialize(targetTile.GridPosition.x, targetTile.GridPosition.y, false);
        
        // 이동 애니메이션
        yield return StartCoroutine(MovePlayer(centerPosition));
        
        // 새로운 위치 점유 설정
        GridManager.Instance.GetTileAtCellPosition(new Vector3Int(targetTile.X, targetTile.Y, 0)).OccupyingUnit = this;
        CurrentTile.SetOccupied(this);

        // hasMovedThisTurn = true;
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
        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, damage.ToString());
        if (!IsAlive)
        {
            Die();
        }
    }

    public void Dodge()
    {
        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, "Miss");
    }
    void Die()
    {
        CurrentTile.ClearOccupied();
        // GridManager에서도 제거
        GridManager.Instance.GetTileAtCellPosition(new Vector3Int(CurrentTile.X, CurrentTile.Y, 0)).OccupyingUnit = null;
        
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

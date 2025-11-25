using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    [SerializeField] private UnitDataSO unitData;
    [SerializeField] public UnitStats stats;
    [SerializeField] private Tilemap tilemap;
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
    
    [SerializeField] private float timeToMove = 3f;

    void Start()
    {
        CurrentHealth = MaxHealth;
        /////////////////////ToDo Test, Delete Later//////////////////
        /// 로직 테스트용 목적으로 구현하지만 나중에는 구조적으로 분리하자.
        Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
        TileBase curTile = tilemap.GetTile<TileBase>(cellPosition);
        if (curTile is TileCustomWithEvent)
        {
            Debug.Log("TileCustomWithEvent found");
            TileCustomWithEvent tileWithEvent = curTile as TileCustomWithEvent;
            Initialize(tileWithEvent);
        }
        Debug.Log(cellPosition);
        
        // 셀의 중심 월드 좌표를 가져옴
        Vector3 centerPosition = tilemap.GetCellCenterWorld(cellPosition);
        Debug.Log(centerPosition);
        
        // 오브젝트를 중심으로 이동
        //transform.position = new Vector3(centerPosition.x, centerPosition.y +1.25f, transform.position.z);
        //transform.position = new Vector3(0,0,0);
        Debug.Log(transform.position);
        StartCoroutine(MovePlayer(centerPosition));
        ///////////////////////////////////
        /// ToDo 주변 타일 검사해서 위치 이동 및 적 존재 유무 확인
    }
    
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
    
    public IEnumerator MoveTo(TileCustomWithEvent targetTile)
    {
        if (!CanMove()) yield break;
        
        List<TileCustomWithEvent> path = Pathfinding.Instance.FindPath(CurrentTile, targetTile);
        
        if (path != null)
        {
            CurrentTile.ClearOccupied();
            
            foreach (TileCustomWithEvent tile in path)
            {
                //transform.position = tile.transform.position;
                yield return new WaitForSeconds(0.2f);
            }
            
            CurrentTile = targetTile;
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
        
        if (!IsAlive)
        {
            Die();
        }
    }
    
    void Die()
    {
        CurrentTile.ClearOccupied();
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
    
    IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;
        
        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = direction;
        targetPosition.y = targetPosition.y + 1.25f; // Adjust for unit height
        
        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsedTime / timeToMove
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        isMoving = false;
    }
}
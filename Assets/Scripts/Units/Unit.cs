using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitDataSO unitData;
    
    [SerializeField] private Tilemap tilemap;
    public string UnitName => unitData.unitName;
    public int CurrentHealth { get; private set; }
    public int MaxHealth => unitData.maxHealth;
    public int AttackPower => unitData.attack;
    public int MovementRange => unitData.movementRange;
    public bool IsAlive => CurrentHealth > 0;
    
    public Tile CurrentTile { get; private set; }
    
    private bool hasMovedThisTurn = false;
    private bool hasAttackedThisTurn = false;
    private bool isMoving;
    
    [SerializeField] private float timeToMove = 3f;

    void Start()
    {
        CurrentHealth = MaxHealth;
        /////////////////////ToDo Test, Delete Later//////////////////
        Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
        TileCustomWithEvent curTile = tilemap.GetTile<TileCustomWithEvent>(cellPosition);
        //Initialize(curTile);
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
    
    public void Initialize(Tile startTile)
    {
        CurrentTile = startTile;
        startTile.SetOccupied(this);
        transform.position = startTile.transform.position;
    }
    
    public void ResetTurnActions()
    {
        hasMovedThisTurn = false;
        hasAttackedThisTurn = false;
    }
    
    public bool CanMove() => !hasMovedThisTurn && IsAlive;
    public bool CanAttack() => !hasAttackedThisTurn && IsAlive;
    
    public List<Tile> GetMovableTiles()
    {
        List<Tile> movableTiles = new List<Tile>();
        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();
        
        queue.Enqueue(CurrentTile);
        visited.Add(CurrentTile);
        CurrentTile.GCost = 0;
        
        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            
            foreach (Tile neighbor in GridManager.Instance.GetNeighbors(current))
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
    
    public IEnumerator MoveTo(Tile targetTile)
    {
        if (!CanMove()) yield break;
        
        List<Tile> path = Pathfinding.Instance.FindPath(CurrentTile, targetTile);
        
        if (path != null)
        {
            CurrentTile.ClearOccupied();
            
            foreach (Tile tile in path)
            {
                transform.position = tile.transform.position;
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
                List<Tile> movableTiles = GetMovableTiles();
                Tile closestTile = GetClosestTileToTarget(movableTiles, target.CurrentTile);
                
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
    
    Tile GetClosestTileToTarget(List<Tile> tiles, Tile targetTile)
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
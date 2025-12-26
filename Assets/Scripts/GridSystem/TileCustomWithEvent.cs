using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Event Tile", menuName = "Tiles/Event Tile")]
public class TileCustomWithEvent : TileCustom
{
    // 기본 정보
    public int TileType; // 0: 평지, 1: 산, 2: 물 등등
    
    public int X { get; private set; }
    public int Y { get; private set; }
    public Vector2Int GridPosition => new Vector2Int(X, Y);
    public bool IsWalkable { get; private set; }
    public Unit OccupyingUnit { get; set; }
    
    
    // A* Pathfinding 용
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost => GCost + HCost;
    public TileCustomWithEvent Parent { get; set; }
    
    private SpriteRenderer spriteRenderer;

    public TileCustomWithEvent(int x, int y, bool walkable)
    {
        X = x;
        Y = y;
        IsWalkable = walkable;
    }
    
    public void Initialize(int x, int y, bool walkable)
    {
        X = x;
        Y = y;
        IsWalkable = walkable;
        //spriteRenderer = GetComponent<SpriteRenderer>();      // TODO TileBase에는 SpriteRenderer가 없음
    }
    
    public void SetOccupied(Unit unit)
    {
        OccupyingUnit = unit;
    }
    
    public void ClearOccupied()
    {
        OccupyingUnit = null;
    }
    
    public void Highlight(Color color)
    {
        spriteRenderer.color = color;
    }
    
    public void ResetHighlight()
    {
        spriteRenderer.color = Color.white;
    }
}
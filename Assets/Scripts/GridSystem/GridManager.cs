using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 특정 셀 위치의 타일 데이터를 저장하는 클래스
/// </summary>
public class TileData
{
    public Vector3Int CellPosition { get; set; }
    public TileCustomWithEvent TileAsset { get; set; }
    public Unit OccupyingUnit { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost => GCost + HCost;
    public TileData Parent { get; set; }
    
    public int X => CellPosition.x;
    public int Y => CellPosition.y;
    public Vector2Int GridPosition => new Vector2Int(X, Y);
    public bool IsWalkable => TileAsset != null && TileAsset.IsWalkable;
    public int TileType => TileAsset != null ? TileAsset.TileType : 0;
}

public class GridManager: MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private bool isPointyTopHex = true; // true: Pointy-top, false: Flat-top
    
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject tilePrefab;
    
    private TileCustomWithEvent[,] grid;
    private Dictionary<Vector2Int, TileCustomWithEvent> tileDict;
    
    // 셀 좌표별 타일 데이터 관리
    private Dictionary<Vector3Int, TileData> tileDataDict = new Dictionary<Vector3Int, TileData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 월드 좌표를 기반으로 타일을 가져옵니다
    /// </summary>
    public TileCustomWithEvent GetTileAtWorldPosition(Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return GetTileAtCellPosition(cellPosition);
    }
    
    /// <summary>
    /// 셀 좌표(Vector3Int)를 기반으로 타일 데이터를 가져옵니다
    /// </summary>
    public TileData GetTileDataAtCellPosition(Vector3Int cellPosition)
    {
        if (tileDataDict.TryGetValue(cellPosition, out TileData tileData))
        {
            return tileData;
        }
        
        // 타일맵에서 타일을 가져와 데이터 생성
        TileBase tile = tilemap.GetTile(cellPosition);
        if (tile is TileCustomWithEvent customTile)
        {
            tileData = new TileData
            {
                CellPosition = cellPosition,
                TileAsset = customTile,
                OccupyingUnit = null
            };
            tileDataDict[cellPosition] = tileData;
            return tileData;
        }
        
        return null;
    }
    
    /// <summary>
    /// 셀 좌표(Vector3Int)를 기반으로 타일을 가져옵니다 (기존 호환성 유지)
    /// </summary>
    public TileCustomWithEvent GetTileAtCellPosition(Vector3Int cellPosition)
    {
        TileBase tile = tilemap.GetTile(cellPosition);
        if (tile is TileCustomWithEvent customTile)
        {
            // 타일 좌표를 셀 좌표로 강제 동기화
            customTile.Initialize(cellPosition.x, cellPosition.y, true);
            return customTile;
        }
        return null;
    }
    
    /// <summary>
    /// 그리드 좌표(x, y)를 기반으로 타일을 가져옵니다
    /// </summary>
    public TileCustomWithEvent GetTile(int x, int y)
    {
        Vector3Int cellPosition = new Vector3Int(x, y, 0);
        return GetTileAtCellPosition(cellPosition);
    }
    
    public TileCustomWithEvent GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);
    
    /// <summary>
    /// 주어진 타일의 이웃 타일들을 반환합니다 (Hexagon 그리드 지원)
    /// </summary>
    public List<TileCustomWithEvent> GetNeighbors(TileCustomWithEvent tile)
    {
        List<TileCustomWithEvent> neighbors = new List<TileCustomWithEvent>();
        
        Vector3Int currentCell = new Vector3Int(tile.X, tile.Y, 0);
        
        // Hexagonal 타일맵의 6방향 이웃
        Vector3Int[] hexDirections = GetHexDirections(currentCell.y);
        
        foreach (var dir in hexDirections)
        {
            Vector3Int neighborCell = currentCell + dir;
            TileCustomWithEvent neighbor = GetTileAtCellPosition(neighborCell);
            
            if (neighbor != null && neighbor.IsWalkable)
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// 주어진 타일 데이터의 이웃 타일 데이터들을 반환합니다
    /// </summary>
    public List<TileData> GetNeighborsData(TileData tileData)
    {
        List<TileData> neighbors = new List<TileData>();
        
        Vector3Int currentCell = tileData.CellPosition;
        
        // Hexagonal 타일맵의 6방향 이웃
        Vector3Int[] hexDirections = GetHexDirections(currentCell.y);
        
        foreach (var dir in hexDirections)
        {
            Vector3Int neighborCell = currentCell + dir;
            TileData neighbor = GetTileDataAtCellPosition(neighborCell);
            
            if (neighbor != null && neighbor.IsWalkable)
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// Hexagonal 그리드의 6방향 오프셋을 반환합니다
    /// Offset 좌표계에서 홀수/짝수 행에 따라 다른 오프셋 사용
    /// </summary>
    private Vector3Int[] GetHexDirections(int row)
    {
        if (isPointyTopHex)
        {
            // Pointy-top hexagon (Offset 좌표계)
            if (row % 2 == 0) // 짝수 행
            {
                return new Vector3Int[]
                {
                    new Vector3Int(1, 0, 0),   // 우
                    new Vector3Int(0, 1, 0),   // 우상
                    new Vector3Int(-1, 1, 0),  // 좌상
                    new Vector3Int(-1, 0, 0),  // 좌
                    new Vector3Int(-1, -1, 0), // 좌하
                    new Vector3Int(0, -1, 0)   // 우하
                };
            }
            else // 홀수 행
            {
                return new Vector3Int[]
                {
                    new Vector3Int(1, 0, 0),   // 우
                    new Vector3Int(1, 1, 0),   // 우상
                    new Vector3Int(0, 1, 0),   // 좌상
                    new Vector3Int(-1, 0, 0),  // 좌
                    new Vector3Int(0, -1, 0),  // 좌하
                    new Vector3Int(1, -1, 0)   // 우하
                };
            }
        }
        else
        {
            // Flat-top hexagon (Offset 좌표계)
            if (row % 2 == 0) // 짝수 열
            {
                return new Vector3Int[]
                {
                    new Vector3Int(1, 0, 0),   // 우상
                    new Vector3Int(1, -1, 0),  // 우하
                    new Vector3Int(0, -1, 0),  // 하
                    new Vector3Int(-1, -1, 0), // 좌하
                    new Vector3Int(-1, 0, 0),  // 좌상
                    new Vector3Int(0, 1, 0)    // 상
                };
            }
            else // 홀수 열
            {
                return new Vector3Int[]
                {
                    new Vector3Int(1, 1, 0),   // 우상
                    new Vector3Int(1, 0, 0),   // 우하
                    new Vector3Int(0, -1, 0),  // 하
                    new Vector3Int(-1, 0, 0),  // 좌하
                    new Vector3Int(-1, 1, 0),  // 좌상
                    new Vector3Int(0, 1, 0)    // 상
                };
            }
        }
    }
    
    /// <summary>
    /// 타일맵의 모든 타일 정보를 가져옵니다 (디버그/초기화용)
    /// </summary>
    public void InitializeAllTilesFromTilemap()
    {
        BoundsInt bounds = tilemap.cellBounds;
        
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(cellPosition);
                
                if (tile is TileCustomWithEvent customTile)
                {
                    customTile.Initialize(x, y, true);
                    Debug.Log($"Tile initialized at ({x}, {y})");
                }
            }
        }
    }
}
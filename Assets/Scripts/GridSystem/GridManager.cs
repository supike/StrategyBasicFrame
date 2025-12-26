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
    
    // 제거: 공유된 에셋 배열/딕셔너리는 오해 소지가 있어 사용하지 않음
    // private TileCustomWithEvent[,] grid;
    // private Dictionary<Vector2Int, TileCustomWithEvent> tileDict;

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
        // 기존처럼 에셋의 X/Y를 변경하면 공유 에셋이 오염되므로 Initialize 호출 제거
        TileBase tile = tilemap.GetTile(cellPosition);
        if (tile is TileCustomWithEvent customTile)
        {
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
// 에셋만으로는 위치를 알기 어려우므로 tileDataDict에서 해당 에셋이 사용된 셀을 찾아 사용
        // 동일 에셋이 여러 셀에 쓰인다면 첫 매칭을 사용함(권장: TileData로 호출)
        TileData found = null;
        foreach (var kv in tileDataDict)
        {
            if (kv.Value.TileAsset == tile)
            {
                found = kv.Value;
                break;
            }
        }

        if (found == null)
        {
            // fallback: 에셋에 저장된 X/Y를 사용하되, 이 값은 신뢰할 수 없음
            Debug.LogWarning("GetNeighbors: 해당 에셋의 TileData를 찾을 수 없습니다. TileData 기반 호출을 권장합니다.");
            Vector3Int currentCellFallback = new Vector3Int(tile.X, tile.Y, 0);
            Vector3Int[] hexDirectionsFallback = GetHexDirections(currentCellFallback.y);
            foreach (var dir in hexDirectionsFallback)
            {
                Vector3Int neighborCell = currentCellFallback + dir;
                TileBase neighborBase = tilemap.GetTile(neighborCell);
                if (neighborBase is TileCustomWithEvent neighborTile && neighborTile.IsWalkable)
                {
                    //todo : 타일 SO로 인한 변수 중복 참조 문제 해결 필요
                    neighbors.Add(neighborTile);
                }
            }
            return neighbors;
        }

        Vector3Int currentCell = found.CellPosition;
        Vector3Int[] hexDirections = GetHexDirections(currentCell.y);

        foreach (var dir in hexDirections)
        {
            Vector3Int neighborCell = currentCell + dir;
            TileData neighborData = GetTileDataAtCellPosition(neighborCell);

            if (neighborData != null && neighborData.IsWalkable)
            {
                neighbors.Add(neighborData.TileAsset);
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
                    // 에셋을 직접 초기화하지 않고 TileData로 관리
                    if (!tileDataDict.ContainsKey(cellPosition))
                    {
                        var data = new TileData
                        {
                            CellPosition = cellPosition,
                            TileAsset = customTile,
                            OccupyingUnit = null
                        };
                        tileDataDict[cellPosition] = data;
                    }
                    // customTile.Initialize(x, y, true);
                    Debug.Log($"Tile initialized at ({x}, {y})");
                }
            }
        }
    }
}
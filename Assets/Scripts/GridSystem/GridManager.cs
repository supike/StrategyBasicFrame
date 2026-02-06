using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager: MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private bool isPointyTopHex = true; // true: Pointy-top, false: Flat-top
    
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject tilePrefab;
    
    // 셀 좌표별 타일 인스턴스 관리
    private Dictionary<Vector3Int, TileCustomWithEvent> tileDict = new Dictionary<Vector3Int, TileCustomWithEvent>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAllTilesFromTilemap();
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
    /// 셀 좌표(Vector3Int)를 기반으로 타일 인스턴스를 가져옵니다
    /// </summary>
    public TileCustomWithEvent GetTileAtCellPosition(Vector3Int cellPosition)
    {
        if (tileDict.TryGetValue(cellPosition, out TileCustomWithEvent tile))
        {
            return tile;
        }
        
        // 딕셔너리에 없으면 타일맵에서 가져와서 인스턴스화 후 저장 (Lazy Initialization)
        TileBase tileBase = tilemap.GetTile(cellPosition);
        if (tileBase is TileCustomWithEvent customTileAsset)
        {
            TileCustomWithEvent tileInstance = Instantiate(customTileAsset);
            tileInstance.Initialize(cellPosition.x, cellPosition.y, true);
            tileDict[cellPosition] = tileInstance;
            return tileInstance;
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
    /// 주어진 타일의 이웃 타일들을 반환합니다
    /// </summary>
    public List<TileCustomWithEvent> GetNeighbors(TileCustomWithEvent tile)
    {
        Vector3Int currentCell = new Vector3Int(tile.X, tile.Y, 0);
        return GetNeighbors(currentCell);
    }

    public List<TileCustomWithEvent> GetNeighbors(Vector3Int cellPosition)
    {
        List<TileCustomWithEvent> neighbors = new List<TileCustomWithEvent>();
        Vector3Int[] hexDirections = GetHexDirections(cellPosition.y);

        foreach (var dir in hexDirections)
        {
            Vector3Int neighborCell = cellPosition + dir;
            TileCustomWithEvent neighbor = GetTileAtCellPosition(neighborCell);

            if (neighbor != null && neighbor.IsWalkable)
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }
    
    /// <summary>
    /// Hexagonal 그리드의 6방향 오프셋을 반환합니다
    /// </summary>
    private Vector3Int[] GetHexDirections(int row)
    {
        if (isPointyTopHex)
        {
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
    /// 타일맵의 모든 타일 정보를 가져옵니다
    /// </summary>
    public void InitializeAllTilesFromTilemap()
    {
        tileDict.Clear();
        BoundsInt bounds = tilemap.cellBounds;
        
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(cellPosition);
                
                if (tile is TileCustomWithEvent customTileAsset)
                {
                    if (!tileDict.ContainsKey(cellPosition))
                    {
                        TileCustomWithEvent tileInstance = Instantiate(customTileAsset);
                        tileInstance.Initialize(x, y, true);
                        tileDict[cellPosition] = tileInstance;
                    }
                    // Debug.Log($"Tile initialized at ({x}, {y})");
                }
            }
        }
    }
}
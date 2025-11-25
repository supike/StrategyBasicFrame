
using System.Collections.Generic;
using UnityEngine;

public class GridManager: MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject tilePrefab;
    
    private TileCustomWithEvent[,] grid;
    private Dictionary<Vector2Int, TileCustomWithEvent> tileDict;
    
    void Start()
    {
        GenerateGrid();
    }
    
    void GenerateGrid()
    {
        grid = new TileCustomWithEvent[width, height];
        tileDict = new Dictionary<Vector2Int, TileCustomWithEvent>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(x * cellSize, y * cellSize, 0);
                GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                
                TileCustomWithEvent tile = tileObj.GetComponent<TileCustomWithEvent>();
                tile.Initialize(x, y, true); // x, y, isWalkable
                
                grid[x, y] = tile;
                tileDict[new Vector2Int(x, y)] = tile;
            }
        }
    }
    
    public TileCustomWithEvent GetTile(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return grid[x, y];
        return null;
    }
    
    public TileCustomWithEvent GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);
    
    public List<TileCustomWithEvent> GetNeighbors(TileCustomWithEvent tile)
    {
        List<TileCustomWithEvent> neighbors = new List<TileCustomWithEvent>();
        Vector2Int[] directions = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };
        
        foreach (var dir in directions)
        {
            TileCustomWithEvent neighbor = GetTile(tile.GridPosition + dir);
            if (neighbor != null && neighbor.IsWalkable)
                neighbors.Add(neighbor);
        }
        
        return neighbors;
    }
}
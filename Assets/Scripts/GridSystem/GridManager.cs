
using System.Collections.Generic;
using UnityEngine;

public class GridManager: MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject tilePrefab;
    
    private Tile[,] grid;
    private Dictionary<Vector2Int, Tile> tileDict;
    
    void Start()
    {
        GenerateGrid();
    }
    
    void GenerateGrid()
    {
        grid = new Tile[width, height];
        tileDict = new Dictionary<Vector2Int, Tile>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(x * cellSize, y * cellSize, 0);
                GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                
                Tile tile = tileObj.GetComponent<Tile>();
                tile.Initialize(x, y, true); // x, y, isWalkable
                
                grid[x, y] = tile;
                tileDict[new Vector2Int(x, y)] = tile;
            }
        }
    }
    
    public Tile GetTile(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return grid[x, y];
        return null;
    }
    
    public Tile GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);
    
    public List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };
        
        foreach (var dir in directions)
        {
            Tile neighbor = GetTile(tile.GridPosition + dir);
            if (neighbor != null && neighbor.IsWalkable)
                neighbors.Add(neighbor);
        }
        
        return neighbors;
    }
}
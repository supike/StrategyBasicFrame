using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    static public Pathfinding Instance { get; private set; }
    private GridManager gridManager;
    
    void Start()
    {
        gridManager = GridManager.Instance;
    }
    
    public List<Tile> FindPath(Tile startTile, Tile targetTile)
    {
        List<Tile> openSet = new List<Tile>();
        HashSet<Tile> closedSet = new HashSet<Tile>();
        
        openSet.Add(startTile);
        
        while (openSet.Count > 0)
        {
            Tile currentTile = openSet[0];
            
            // 가장 낮은 FCost를 가진 타일 찾기
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentTile.FCost ||
                    (openSet[i].FCost == currentTile.FCost && openSet[i].HCost < currentTile.HCost))
                {
                    currentTile = openSet[i];
                }
            }
            
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);
            
            // 목표 도달
            if (currentTile == targetTile)
            {
                return RetracePath(startTile, targetTile);
            }
            
            // 이웃 타일 탐색
            foreach (Tile neighbor in gridManager.GetNeighbors(currentTile))
            {
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                    continue;
                
                int newGCost = currentTile.GCost + GetDistance(currentTile, neighbor);
                
                if (newGCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newGCost;
                    neighbor.HCost = GetDistance(neighbor, targetTile);
                    neighbor.Parent = currentTile;
                    
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        
        return null; // 경로 없음
    }
    
    List<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = endTile;
        
        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.Parent;
        }
        
        path.Reverse();
        return path;
    }
    
    int GetDistance(Tile tileA, Tile tileB)
    {
        int distX = Mathf.Abs(tileA.X - tileB.X);
        int distY = Mathf.Abs(tileA.Y - tileB.Y);
        return distX + distY; // 맨해튼 거리
    }
}
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
    
    public List<TileCustomWithEvent> FindPath(TileCustomWithEvent startTile, TileCustomWithEvent targetTile)
    {
        List<TileCustomWithEvent> openSet = new List<TileCustomWithEvent>();
        HashSet<TileCustomWithEvent> closedSet = new HashSet<TileCustomWithEvent>();
        
        openSet.Add(startTile);
        
        while (openSet.Count > 0)
        {
            TileCustomWithEvent currentTile = openSet[0];
            
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
            foreach (TileCustomWithEvent neighbor in gridManager.GetNeighbors(currentTile))
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
    
    List<TileCustomWithEvent> RetracePath(TileCustomWithEvent startTile, TileCustomWithEvent endTile)
    {
        List<TileCustomWithEvent> path = new List<TileCustomWithEvent>();
        TileCustomWithEvent currentTile = endTile;
        
        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.Parent;
        }
        
        path.Reverse();
        return path;
    }
    
    int GetDistance(TileCustomWithEvent tileA, TileCustomWithEvent tileB)
    {
        int distX = Mathf.Abs(tileA.X - tileB.X);
        int distY = Mathf.Abs(tileA.Y - tileB.Y);
        return distX + distY; // 맨해튼 거리
    }
}
# 경로 찾기(Pathfinding) 구현 가이드

## 개요
게임에서 캐릭터가 목표지점까지 효율적으로 이동하기 위해 경로를 찾는 알고리즘입니다. 
BattleManager에서는 **A* (A-Star) 알고리즘**을 사용합니다.

---

## 흔히 사용되는 경로 찾기 알고리즘

### 1. **BFS (Breadth-First Search) - 너비 우선 탐색**
```
장점:
- 구현이 간단함
- 최단 경로 보장
- 모든 타일이 동일한 비용일 때 적합

단점:
- 목표 방향을 고려하지 않음
- 불필요한 타일까지 탐색
- 큰 맵에서 성능 저하

사용 예:
GetMovableTiles() - 이동 가능한 모든 타일 탐색
```

### 2. **Dijkstra (다익스트라)** 
```
장점:
- 다양한 비용을 고려 가능 (높이, 지형 등)
- 최단 경로 보장

단점:
- BFS보다 느림
- 휴리스틱이 없음
- 목표를 향해 진행하지 않음

사용 예:
복잡한 지형이 있는 큰 맵
```

### 3. **Greedy Best-First Search**
```
장점:
- 매우 빠름
- 구현이 간단

단점:
- 최단 경로를 보장하지 않음
- 막혔을 때 대체 경로를 찾지 못함
- 전략 게임에는 부적합

사용 예:
간단한 캐주얼 게임
```

### 4. **A* (A-Star) ⭐ [우리가 사용]**
```
장점:
- Dijkstra의 최적성 + Greedy의 속도
- 휴리스틱을 사용한 방향 기반 탐색
- 대부분의 게임에서 최고 성능
- 최단 경로 보장

단점:
- 구현이 복잡함
- 휴리스틱 함수 선택이 중요

사용 예:
전략 게임, RPG, RTS 등
```

---

## A* 알고리즘의 동작 원리

### 기본 개념
```
F = G + H

G: Start에서 현재 노드까지의 실제 비용 (가온 거리)
H: 현재 노드에서 Goal까지의 예상 비용 (휴리스틱)
F: 전체 예상 비용
```

### 동작 순서
```
1. OpenSet에 시작 노드 추가
2. OpenSet에서 F값이 가장 작은 노드 선택
3. 그것이 목표 노드면 경로 역추적하여 반환
4. ClosedSet에 현재 노드 추가
5. 인접한 모든 노드에 대해:
   - 이미 ClosedSet에 있으면 스킵
   - 새로운 G값 계산
   - 더 좋은 경로면 업데이트
6. 2번으로 돌아가기
```

### 시각적 예시
```
Start → [탐색 영역] ← Goal

G값: 검정색 숫자 (이미 간 거리)
H값: 회색 숫자 (예상 거리)
F값: 합계

┌─────────┬─────────┬─────────┐
│ G=2 H=7 │ G=2 H=6 │ G=2 H=5 │
│ F=9     │ F=8     │ F=7     │
├─────────┼─────────┼─────────┤
│ G=1 H=8 │ G=1 H=7 │ G=1 H=6 │
│ F=9     │ F=8     │ F=7  ⭐ │ ← 다음 탐색할 노드
├─────────┼─────────┼─────────┤
│ G=0 H=9 │ G=1 H=8 │ G=2 H=7 │
│ F=9     │ F=9     │ F=9     │
│  START  │         │         │
└─────────┴─────────┴─────────┘
```

---

## 코드 구현

### 1. 휴리스틱 함수 (Heuristic Function)
```csharp
// 맨해튼 거리 (격자형 맵, 4방향 이동)
private float Heuristic(Vector2Int from, Vector2Int to)
{
    return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
}

// 대각선 이동을 고려한 휴리스틱 (8방향 이동)
private float HeuristicDiagonal(Vector2Int from, Vector2Int to)
{
    int dx = Mathf.Abs(from.x - to.x);
    int dy = Mathf.Abs(from.y - to.y);
    return Mathf.Max(dx, dy);  // 체비셰프 거리
}

// 유클리드 거리 (직선 이동, 정확도 높음)
private float HeuristicEuclidean(Vector2Int from, Vector2Int to)
{
    return Vector2.Distance(from, to);
}
```

### 2. PathNode 클래스
```csharp
private class PathNode
{
    public TileCustomWithEvent Tile { get; set; }
    public PathNode Parent { get; set; }
    public float G { get; set; }  // 비용
    public float H { get; set; }  // 휴리스틱
    public float F => G + H;      // 전체 비용

    public PathNode(TileCustomWithEvent tile, PathNode parent, float g, float h)
    {
        Tile = tile;
        Parent = parent;
        G = g;
        H = h;
    }
}
```

### 3. A* 경로 찾기 함수
```csharp
private TileCustomWithEvent FindPathToTarget(TileCustomWithEvent targetTile)
{
    List<PathNode> openSet = new List<PathNode>();
    HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
    Dictionary<Vector2Int, PathNode> allNodes = new Dictionary<Vector2Int, PathNode>();

    Vector2Int startPos = CurrentTile.GridPosition;
    Vector2Int targetPos = targetTile.GridPosition;

    PathNode startNode = new PathNode(CurrentTile, null, 0, Heuristic(startPos, targetPos));
    openSet.Add(startNode);
    allNodes[startPos] = startNode;

    while (openSet.Count > 0)
    {
        // F값이 가장 작은 노드 찾기
        int current = 0;
        for (int i = 1; i < openSet.Count; i++)
        {
            if (openSet[i].F < openSet[current].F)
                current = i;
        }

        PathNode currentNode = openSet[current];

        // 목표 도달
        if (currentNode.Tile.GridPosition == targetPos)
        {
            return GetFirstStepToTarget(currentNode);
        }

        openSet.RemoveAt(current);
        closedSet.Add(currentNode.Tile.GridPosition);

        // 인접 타일 탐색
        foreach (TileCustomWithEvent neighbor in GridManager.Instance.GetNeighbors(...))
        {
            Vector2Int neighborPos = neighbor.GridPosition;

            if (closedSet.Contains(neighborPos)) continue;
            if (neighbor.OccupyingUnit != null && neighbor.OccupyingUnit != this) continue;

            float newG = currentNode.G + 1;

            if (allNodes.ContainsKey(neighborPos))
            {
                PathNode existing = allNodes[neighborPos];
                if (newG < existing.G)
                {
                    existing.Parent = currentNode;
                    existing.G = newG;
                }
            }
            else
            {
                float h = Heuristic(neighborPos, targetPos);
                PathNode newNode = new PathNode(neighbor, currentNode, newG, h);
                openSet.Add(newNode);
                allNodes[neighborPos] = newNode;
            }
        }
    }

    return null;  // 경로 없음
}
```

---

## 성능 최적화 팁

### 1. 우선순위 큐 사용
```csharp
// 현재 (List에서 선형 탐색): O(n)
// 우선순위 큐 사용: O(log n)

private MinHeap<PathNode> openSet = new MinHeap<PathNode>();
```

### 2. 맵 크기 고려
- 작은 맵 (< 20x20): BFS 충분
- 중간 맵 (20x100): A* 권장
- 큰 맵 (100x100+): JPS (Jump Point Search) 고려

### 3. 캐싱
```csharp
// 같은 목표로 자주 이동하면 경로 캐싱
private Dictionary<Vector2Int, TileCustomWithEvent> pathCache;
```

### 4. 직선 경로 체크
```csharp
// A*를 실행하기 전에 직선 경로가 가능한지 확인
if (CanReachDirectly(target.CurrentTile))
{
    return GetDirectPath(target.CurrentTile);
}
```

---

## 일반적인 문제와 해결책

### 문제: 캐릭터가 같은 위치를 반복
**원인**: 경로 찾기 함수가 null 반환

**해결**:
1. ✅ GetClosestTileToTarget 제대로 구현 (A* 사용)
2. ✅ CurrentTile이 GridManager와 동기화되는지 확인
3. ✅ hasMovedThisTurn 플래그 올바르게 설정

### 문제: 경로 찾기가 너무 느림
**원인**: 맵이 크거나 알고리즘이 비효율적

**해결**:
1. 휴리스틱 함수 검토 (너무 낙관적이면 비효율)
2. 우선순위 큐로 변경
3. JPS 알고리즘 고려
4. 경로 캐싱 추가

### 문제: 경로가 이상함 (지그재그)
**원인**: 휴리스틱이 부정확

**해결**:
1. 맨해튼 거리 대신 체비셰프 거리 사용 (8방향)
2. 각 타일의 비용 확인
3. 장애물 감지 로직 검토

---

## 참고 자료

- **Dijkstra's Algorithm**: 모든 엣지 비용이 양수일 때 최단 경로
- **BFS**: 모든 엣지 비용이 같을 때 최단 경로
- **A***: 휴리스틱이 있는 최적 경로 찾기
- **JPS (Jump Point Search)**: A*의 개선 버전, 대규모 맵에서 10배 빠름

---

## 다음 단계

현재 코드(`Unit.cs`)에서:
1. ✅ A* 알고리즘 구현 완료
2. ✅ FindPathToTarget() 함수 추가
3. ⏳ 우선순위 큐 최적화 (선택사항)
4. ⏳ 경로 캐싱 추가 (선택사항)

경로 찾기가 정상 작동하는지 테스트하세요!


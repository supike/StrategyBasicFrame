# 🚀 경로 찾기 최적화 고급 가이드

## 📌 개요
A* 알고리즘이 정상 작동하는 것을 확인했으므로, 추가 성능 최적화를 위한 가이드입니다.

---

## 1️⃣ 우선순위 큐(Priority Queue) 도입

### 현재 방식의 문제
```csharp
// O(n) - 매번 전체 리스트를 순회하여 최소값 찾기
int current = 0;
for (int i = 1; i < openSet.Count; i++)
{
    if (openSet[i].F < openSet[current].F)
        current = i;
}
```

### 개선 방법: MinHeap 구현
```csharp
// MinHeap 클래스
public class MinHeap<T> where T : System.IComparable<T>
{
    private List<T> items = new List<T>();

    public void Enqueue(T item)
    {
        items.Add(item);
        int childIndex = items.Count - 1;
        
        while (childIndex > 0)
        {
            int parentIndex = (childIndex - 1) / 2;
            if (items[childIndex].CompareTo(items[parentIndex]) >= 0) break;
            
            (items[childIndex], items[parentIndex]) = (items[parentIndex], items[childIndex]);
            childIndex = parentIndex;
        }
    }

    public T Dequeue()
    {
        T root = items[0];
        items[0] = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);
        
        int index = 0;
        while (true)
        {
            int smallest = index;
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            
            if (left < items.Count && items[left].CompareTo(items[smallest]) < 0)
                smallest = left;
            if (right < items.Count && items[right].CompareTo(items[smallest]) < 0)
                smallest = right;
            if (smallest == index) break;
            
            (items[index], items[smallest]) = (items[smallest], items[index]);
            index = smallest;
        }
        
        return root;
    }

    public int Count => items.Count;
}
```

### 성능 개선
| 연산 | 이전 | 개선 후 |
|------|------|--------|
| OpenSet에서 최소값 찾기 | O(n) | O(log n) |
| 총 시간복잡도 | O(n²) | O(n log n) |
| 메모리 | O(n) | O(n) |

**언제 도입할까?**
- 맵 크기: 100x100 이상
- 경로 계산 빈도: 프레임당 여러 개

---

## 2️⃣ 경로 캐싱(Path Caching)

### 문제
같은 목표로 이동할 때마다 A* 계산 반복

### 해결책
```csharp
public class PathCache
{
    private Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>> cache 
        = new Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>>();

    public bool TryGetPath(Vector2Int from, Vector2Int to, out List<Vector2Int> path)
    {
        return cache.TryGetValue((from, to), out path);
    }

    public void CachePath(Vector2Int from, Vector2Int to, List<Vector2Int> path)
    {
        cache[(from, to)] = new List<Vector2Int>(path);
    }

    public void Clear()
    {
        cache.Clear();
    }

    public void InvalidateFrom(Vector2Int pos)
    {
        // pos에서 출발하는 캐시 제거
        var keysToRemove = cache.Keys.Where(k => k.Item1 == pos).ToList();
        foreach (var key in keysToRemove)
        {
            cache.Remove(key);
        }
    }
}
```

### Unit.cs에서 사용
```csharp
private PathCache pathCache = new PathCache();

private TileCustomWithEvent FindPathToTarget(TileCustomWithEvent targetTile)
{
    Vector2Int startPos = CurrentTile.GridPosition;
    Vector2Int targetPos = targetTile.GridPosition;

    // 캐시 확인
    if (pathCache.TryGetPath(startPos, targetPos, out var cachedPath))
    {
        if (debugPathfinding)
            Debug.Log($"[{UnitName}] 캐시된 경로 사용");
        return GetTileFromPosition(cachedPath[0]);
    }

    // A* 계산...
    // 경로 캐싱
    pathCache.CachePath(startPos, targetPos, fullPath);
}
```

### 효과
- **메모리**: 약간 증가
- **속도**: 2-3배 빠름 (반복 경로)
- **CPU**: 대폭 감소

---

## 3️⃣ JPS (Jump Point Search) - 고급 최적화

### A*의 문제점
대규모 맵(500x500+)에서는 여전히 느림

### JPS 개념
불필요한 노드 스킵 → A*의 10배 빠름

```csharp
private TileCustomWithEvent FindPathJPS(TileCustomWithEvent targetTile)
{
    Vector2Int startPos = CurrentTile.GridPosition;
    Vector2Int targetPos = targetTile.GridPosition;
    
    // 직선 방향으로 점프 가능한 지점 찾기
    foreach (Vector2Int direction in GetDirections())
    {
        TileCustomWithEvent jumpNode = Jump(startPos, direction, targetPos);
        if (jumpNode != null)
        {
            // jumpNode에서 계속 탐색
        }
    }
}

private TileCustomWithEvent Jump(Vector2Int pos, Vector2Int direction, Vector2Int goal)
{
    Vector2Int nextPos = pos + direction;
    
    // 장애물 확인
    if (!IsWalkable(nextPos)) return null;
    
    // 목표 도달
    if (nextPos == goal) return GetTile(nextPos);
    
    // 수평/수직 이동
    if (direction.x != 0 && direction.y == 0)
    {
        // 좌우 이동
        TileCustomWithEvent result = JumpHorizontal(nextPos, direction, goal);
        if (result != null) return result;
    }
    
    // 재귀 점프
    return Jump(nextPos, direction, goal);
}
```

**도입 기준**:
- 맵 크기: 500x500 이상
- 경로 계산: 매우 빈번
- CPU 집약적 작업 필요

---

## 4️⃣ 지형 비용(Terrain Cost) 추가

### 기본 개념
모든 타일의 이동 비용이 같지 않을 수 있음

```csharp
public enum TerrainType
{
    Grass = 1,      // 비용 1
    Water = 2,      // 비용 2
    Mountain = 3,   // 비용 3
    Road = 0,       // 비용 0 (최고 속도)
}

public class TileWithTerrain : TileCustomWithEvent
{
    public TerrainType terrainType = TerrainType.Grass;
    
    public int GetMovementCost()
    {
        return (int)terrainType;
    }
}
```

### A*에 지형 비용 적용
```csharp
float newG = currentNode.G + neighbor.GetMovementCost();  // 이전: + 1
```

---

## 5️⃣ 이동 예측(Movement Prediction)

### 개념
움직이는 적을 추격할 때 미래 위치 예측

```csharp
private Vector2Int PredictTargetPosition(Unit target, float timeAhead = 1.0f)
{
    // 목표의 현재 이동 방향과 속도 기반 미래 위치 예측
    Vector2Int currentPos = target.CurrentTile.GridPosition;
    Vector2Int lastPos = target.LastTile?.GridPosition ?? currentPos;
    Vector2Int direction = currentPos - lastPos;
    
    int stepsAhead = Mathf.RoundToInt(timeAhead / Time.fixedDeltaTime);
    Vector2Int predictedPos = currentPos + (direction * stepsAhead);
    
    return predictedPos;
}

// 사용
Unit target = FindNearestPlayerUnit();
Vector2Int targetPos = PredictTargetPosition(target);
TileCustomWithEvent nextTile = FindPathToTarget(GetTile(targetPos));
```

---

## 6️⃣ 경로 부드럽게 만들기(Path Smoothing)

### 문제
A* 경로가 지그재그 형태

### 해결책: 직선 보정
```csharp
public List<Vector2Int> SmoothPath(List<Vector2Int> path)
{
    if (path.Count <= 2) return path;
    
    List<Vector2Int> smoothed = new List<Vector2Int> { path[0] };
    
    for (int i = 1; i < path.Count - 1; i++)
    {
        Vector2Int prev = path[i - 1];
        Vector2Int curr = path[i];
        Vector2Int next = path[i + 1];
        
        // 직선상에 있으면 스킵
        if (!IsCollinear(prev, curr, next))
        {
            smoothed.Add(curr);
        }
    }
    
    smoothed.Add(path[path.Count - 1]);
    return smoothed;
}

private bool IsCollinear(Vector2Int a, Vector2Int b, Vector2Int c)
{
    // (b-a) × (c-a) == 0 이면 일직선
    int crossProduct = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    return crossProduct == 0;
}
```

---

## 7️⃣ 시야(Line of Sight) 최적화

### 개념
직진 이동 가능하면 직선 경로 사용

```csharp
private TileCustomWithEvent TryDirectPath(TileCustomWithEvent targetTile)
{
    // Bresenham 직선 알고리즘
    List<Vector2Int> line = GetBresenhamLine(CurrentTile.GridPosition, targetTile.GridPosition);
    
    // 모든 타일이 통과 가능한가?
    foreach (Vector2Int pos in line)
    {
        TileCustomWithEvent tile = GridManager.Instance.GetTileAtCellPosition(new Vector3Int(pos.x, pos.y, 0));
        if (tile == null || tile.OccupyingUnit != null)
            return null;  // 직선 경로 불가능
    }
    
    // 직선 경로 사용 가능
    return line.Count > 1 ? GetTileFromPosition(line[1]) : targetTile;
}
```

---

## 🎯 최적화 도입 로드맵

```
Phase 1: 현재 상태 (완료)
├─ ✅ A* 알고리즘 구현
├─ ✅ 기본 경로 찾기 작동
└─ ✅ 디버그 기능

Phase 2: 기본 최적화 (권장)
├─ [ ] 우선순위 큐 도입 (중간 맵)
├─ [ ] 경로 캐싱 (반복 경로)
└─ [ ] 경로 부드럽게 (UI 개선)

Phase 3: 고급 최적화 (필요시)
├─ [ ] JPS 알고리즘 (대규모 맵)
├─ [ ] 지형 비용 (전략 게임)
└─ [ ] 이동 예측 (동적 타겟)

Phase 4: 성능 튜닝
├─ [ ] 프로파일링
├─ [ ] 병목 구간 분석
└─ [ ] 최종 최적화
```

---

## 📊 성능 비교표

| 기법 | 속도 향상 | 구현 난이도 | 메모리 | 추천 상황 |
|------|---------|----------|--------|---------|
| **A*** | 기준 | 중간 | 적음 | 기본 |
| **우선순위 큐** | 3-5배 | 중간 | 적음 | 중간 맵 |
| **경로 캐싱** | 2-3배 | 낮음 | 중간 | 반복 경로 |
| **JPS** | 10배 | 높음 | 적음 | 대규모 맵 |
| **시야 최적화** | 1.5-2배 | 낮음 | 적음 | 개방형 공간 |
| **경로 부드럽게** | - | 낮음 | 적음 | UI 개선 |

---

## 🔧 디버그 팁

### 경로 시각화
```csharp
private void OnDrawGizmos()
{
    if (!debugPathfinding) return;
    
    // 전체 경로 표시
    foreach (var pos in lastCalculatedPath)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(GetWorldPos(pos), Vector3.one * 0.5f);
    }
}
```

### 성능 측정
```csharp
private float MeasurePathfindingTime()
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    TileCustomWithEvent result = FindPathToTarget(targetTile);
    stopwatch.Stop();
    
    Debug.Log($"경로 찾기: {stopwatch.ElapsedMilliseconds}ms");
    return stopwatch.ElapsedMilliseconds;
}
```

---

## 📚 참고 자료

- **우선순위 큐**: https://en.wikipedia.org/wiki/Binary_heap
- **JPS**: https://en.wikipedia.org/wiki/Jump_Point_Search
- **경로 부드럽게**: Catmull-Rom Spline
- **Red Blob Games**: 최고의 경로 찾기 튜토리얼

---

**작성일**: 2026-01-14  
**상태**: 📚 참고용 가이드  
**다음 단계**: 필요시 Phase 2 최적화 도입


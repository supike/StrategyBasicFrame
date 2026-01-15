# 경로 찾기 버그 수정 및 구현 완료 보고서

## 📋 문제 요약

**캐릭터가 같은 위치를 반복해서 배회하는 문제**

### 원인 분석
1. **GetClosestTileToTarget() 미구현**: 항상 `null` 반환
   - AI가 이동할 타일을 찾지 못해 계속 같은 위치에서 대기
   
2. **CurrentTile 동기화 문제**: 
   - `Initialize()` 에서 `Instantiate()` 사용으로 복사본 생성
   - GridManager의 실제 타일과 불일치 → 다음 이동 시 잘못된 위치 기준

3. **GetMovableTiles() 불완전한 구현**:
   - 이동 범위(MovementRange) 미적용
   - 중복 방문 추적 미흡

---

## ✅ 수정 사항

### 1. A* 경로 찾기 알고리즘 구현
```csharp
// FindPathToTarget() - A* 알고리즘으로 목표까지의 최단 경로 계산
// PathNode 클래스 - 경로 노드 데이터 구조
// Heuristic() - 맨해튼 거리 휴리스틱 함수
// GetFirstStepToTarget() - 경로 역추적하여 첫 번째 이동 타일 반환
```

**특징:**
- ✅ 최단 경로 보장
- ✅ 목표 방향 기반 탐색 (효율적)
- ✅ 장애물(다른 유닛) 고려
- ✅ 이동 범위 내 최적 경로 계산

### 2. CurrentTile 동기화 수정
```csharp
// ❌ 이전 (문제있음)
CurrentTile = Instantiate(startTile);  // 복사본 생성
CurrentTile.Initialize(startTile.X, startTile.Y, false);

// ✅ 개선 (해결)
CurrentTile = GridManager.Instance.GetTileAtCellPosition(...)  // 실제 타일 참조
```

### 3. GetMovableTiles() 개선
```csharp
// BFS 알고리즘으로 이동 가능한 모든 타일 탐색
// ✅ 이동 범위(MovementRange) 제한 적용
// ✅ Dictionary로 방문 타일과 비용 추적
// ✅ 타일 중복 방문 방지
```

### 4. ExecuteAI() 로직 개선
```csharp
// ❌ 이전
List<TileCustomWithEvent> movableTiles = GetMovableTiles();
TileCustomWithEvent closestTile = GetClosestTileToTarget(movableTiles, target.CurrentTile);
// GetClosestTileToTarget가 null 반환 → 이동 불가

// ✅ 개선
TileCustomWithEvent nextTile = FindPathToTarget(target.CurrentTile);
// A*로 직접 경로 계산 → 항상 최단 경로 반환
```

### 5. PrepareForAttack() 안정성 개선
```csharp
// ❌ 이전
if (!targetUnit.IsAlive || targetUnit == null)  // NullReferenceException 위험!

// ✅ 개선
if (targetUnit == null || !targetUnit.IsAlive)  // 안전한 체크 순서
```

### 6. CameraController 마우스 우클릭 명확화
```csharp
// ❌ 이전
if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))  // 좌우클릭 모두 작동

// ✅ 개선
if (Input.GetMouseButtonDown(1))  // 우클릭(버튼 1)만 작동
```

---

## 🔍 흔히 사용하는 경로 찾기 방식 비교

| 알고리즘 | 성능 | 최단경로 | 복잡도 | 용도 |
|---------|------|--------|--------|------|
| **BFS** | 보통 | ✅ | 낮음 | 단순 맵, 모든 타일 탐색 |
| **Dijkstra** | 보통 | ✅ | 중간 | 다양한 비용의 경로 |
| **Greedy** | 빠름 | ❌ | 낮음 | 캐주얼 게임 |
| **A*** | 빠름 | ✅ | 높음 | ⭐ RPG, 전략 게임 |
| **JPS** | 매우빠름 | ✅ | 높음 | 대규모 맵 |

### 우리의 선택: A* 알고리즘
- ✅ Dijkstra의 정확성 + Greedy의 속도
- ✅ 휴리스틱(목표 방향) 고려
- ✅ 대부분의 게임에서 최고 성능
- ✅ 구현 복잡도 ↔ 성능 좋은 균형

---

## 🎯 A* 알고리즘 동작 원리

### 핵심 공식
```
F = G + H

G: 시작점에서 현재 노드까지의 실제 비용 (이동한 거리)
H: 현재 노드에서 목표까지의 예상 비용 (휴리스틱)
F: 전체 예상 비용 (우선순위)
```

### 탐색 순서
```
1. OpenSet에 시작 노드 추가
2. F값이 가장 작은 노드 선택
3. 목표 도달? → 경로 반환 (역추적)
4. ClosedSet에 현재 노드 이동
5. 인접 노드 평가:
   - 이미 방문? → 스킵
   - 더 좋은 경로 발견? → 업데이트
   - 새 경로? → OpenSet에 추가
6. 2번으로 돌아가기
```

### 시각적 예시 (체비셰프/맨해튼 거리)
```
OpenSet (미탐색)   ClosedSet (탐색완료)   Wall (장애물)

    1 2 3 4 5
  ┌─────────────┐
1 │ 4 3 2 1 ⭐ │  ← Goal
  ├─────────────┤
2 │ 5 □ □ □ 5 │     □ = 현재 탐색 중
  ├─────────────┤
3 │ 6 ● ● □ 6 │     ● = 이미 탐색함
  ├─────────────┤
4 │ 7 S ■ □ 7 │     ■ = 장애물
  ├─────────────┤
5 │ 8 8 7 6 8 │     S = Start
  └─────────────┘

S → (1,4) → (1,3) → (1,2) → (1,1) → Goal ✅
```

---

## 📊 성능 최적화 팁

### 현재 구현 (충분한 수준)
- ✅ 격자맵 최적화: O(n log n) ~ O(n²)
- ✅ 평균 맵 크기: 100x100 이하
- ✅ 우선순위: List 선형 탐색 (n < 1000일 때 충분)

### 향후 최적화 (선택사항)
1. **우선순위 큐 사용** (O(log n) 성능)
   ```csharp
   Priority_Queue<PathNode, float> openSet;
   ```

2. **경로 캐싱** (반복 이동 최소화)
   ```csharp
   Dictionary<Vector2Int, TileCustomWithEvent> pathCache;
   ```

3. **JPS (Jump Point Search)** (A*의 10배 빠름, 대규모 맵용)

---

## 🧪 디버그 기능

### Inspector에서 활성화
```
Unit Inspector → Debug Pathfinding 체크박스 활성화
```

### 출력 정보
1. **콘솔 로그**
   - 경로 찾기 시작: 시작점 → 목표점
   - 경로 찾음: OpenSet/ClosedSet 크기
   - 경로 실패: 경로 없음 경고

2. **Scene 뷰 시각화** (에디터)
   - 파란색 구: 플레이어 유닛
   - 빨간색 구: 적 유닛
   - 초록색 큐브: 이동 가능 범위

---

## 🔧 코드 사용 예시

### 1. 적 AI 이동
```csharp
// ExecuteAI()에서 사용
Unit target = FindNearestPlayerUnit();
if (target != null)
{
    TileCustomWithEvent nextTile = FindPathToTarget(target.CurrentTile);
    if (nextTile != null && CanMove())
    {
        yield return StartCoroutine(MoveTo(nextTile));
    }
}
```

### 2. 플레이어 이동 명령
```csharp
// 마우스 클릭으로 이동할 때
TileCustomWithEvent targetTile = GetClickedTile();
TileCustomWithEvent nextTile = FindPathToTarget(targetTile);
if (nextTile != null)
{
    yield return StartCoroutine(MoveTo(nextTile));
}
```

### 3. 이동 가능 범위 표시
```csharp
List<TileCustomWithEvent> movableTiles = GetMovableTiles();
foreach (var tile in movableTiles)
{
    // 타일을 초록색으로 표시
}
```

---

## 📈 테스트 체크리스트

- [ ] 적이 플레이어를 향해 움직이는가?
- [ ] 경로가 장애물을 피하는가?
- [ ] 이동 범위를 초과하지 않는가?
- [ ] 무한 루프에 빠지지 않는가?
- [ ] 적이 플레이어를 추격할 때 최단 경로를 찾는가?
- [ ] Console에 경로 찾기 로그가 출력되는가?

---

## 📚 다음 단계

### 즉시 필요
1. ✅ A* 경로 찾기 구현 완료
2. ✅ CurrentTile 동기화 수정 완료
3. ✅ GetMovableTiles() 개선 완료
4. ✅ 디버그 기능 추가 완료

### 향후 개선 (선택사항)
1. 우선순위 큐 도입 (성능 향상)
2. 경로 캐싱 (반복 계산 감소)
3. JPS 알고리즘 고려 (대규모 맵)
4. 지형 비용 고려 (높이, 습지 등)

---

## 🎓 관련 자료

- **A* 알고리즘**: https://en.wikipedia.org/wiki/A*_search_algorithm
- **Pathfinding**: Red Blob Games - Pathfinding Tutorials
- **Game AI**: "AI for Game Developers" by David M. Bourg

---

**작성일**: 2026-01-14  
**상태**: ✅ 완료  
**담당**: GitHub Copilot


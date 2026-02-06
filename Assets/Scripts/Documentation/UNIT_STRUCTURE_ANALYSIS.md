# Unit 관리 구조 분석 및 개선안

## 📋 현재 구조 분석

### 현재 상황
```
GameManager (전역 관리)
  ├── playerUnits: List<Unit>
  ├── enemyUnits: List<Unit>
  └── [문제] Unit 리스트만 관리, 실제 로직은 Unit 클래스에 분산

Unit (MonoBehaviour) - 571줄
  ├── [데이터] UnitStats, unitData, CurrentHealth 등
  ├── [전투] Attack(), TakeDamage(), Dodge()
  ├── [이동] MoveTo(), GetMovableTiles(), ExecuteAI()
  ├── [UI] UpdateHealthUI(), healthText, healthSlider
  ├── [상태] hasMovedThisTurn, hasAttackedThisTurn, isPaused
  └── [기타] Animation, Direction, Mode 등
```

### 현재 구조의 문제점
1. **책임 과다 (Huge God Class)**
   - Unit 클래스가 너무 많은 책임을 가짐
   - 데이터, 전투, 이동, UI, AI가 모두 섞여 있음

2. **테스트 어려움**
   - MonoBehaviour이라서 단위 테스트 불가
   - 씬이 필요함

3. **재사용성 낮음**
   - 다른 매니저들이 직접 Unit을 접근
   - 데이터와 로직이 강하게 결합

4. **상태 관리 복잡**
   - hasMovedThisTurn, hasAttackedThisTurn, isPaused 등이 산재
   - 상태 전환 로직이 분명하지 않음

---

## ✅ 추천: 개선된 구조

### 옵션 1: **경량 구조 (추천, 현재 상황에 최적)**
```
GameManager
  ├── UnitManager (새로 추가)
  │   ├── playerUnits
  │   ├── enemyUnits
  │   ├── GetNearestUnit()
  │   ├── GetUnitById()
  │   └── UpdateUnitHealth()
  └──

Unit (슬림화된 571줄 → 200줄)
  ├── [데이터만] UnitStats, CurrentHealth (Getter/Setter)
  ├── [Transform] Position, Direction
  └── [Reference] 필요한 컴포넌트만 캐싱
  
CombatManager → UnitManager 활용
MapManager → UnitManager 활용
TurnManager → UnitManager 활용
```

### 옵션 2: **완전 분리 구조 (규모가 커질 때)**
```
GameManager
  └── UnitManager
        ├── Unit (데이터 클래스, MonoBehaviour 아님)
        │   ├── id, name, stats
        │   └── Getter/Setter만
        │
        ├── UnitCombatComponent (전투 로직)
        ├── UnitMovementComponent (이동 로직)
        ├── UnitAIComponent (AI 로직)
        └── UnitUIComponent (UI 로직)

View Layer (MonoBehaviour)
  └── UnitView
        ├── animator, sprite, UI 요소
        └── Unit 데이터와 동기화
```

---

## 🎯 추천: 옵션 1 구현 (현재 상황에 맞음)

### 이유
1. **현재 구조와의 호환성** - 기존 코드 최소 변경
2. **즉시 효과** - 중복 코드 제거, 관리 용이
3. **점진적 확장** - 나중에 필요하면 옵션 2로 전환 가능
4. **학습 곡선 낮음** - 새로운 개념 적게 도입

### 구현 로드맵
```
1단계: UnitManager 클래스 생성 (유닛 조회, 필터링)
2단계: GameManager에 UnitManager 통합
3단계: CombatManager → UnitManager 사용하도록 수정
4단계: Unit 클래스에서 UI/Animation 로직 일부 추출 (선택)
```

---

## 📊 비교표

| 항목 | 현재 구조 | 옵션 1 | 옵션 2 |
|------|---------|-------|--------|
| 구현 난이도 | 낮음 | 중간 | 높음 |
| 성능 | 보통 | 우수 | 우수 |
| 확장성 | 낮음 | 중간 | 높음 |
| 테스트 용이성 | 낮음 | 중간 | 높음 |
| 학습 시간 | 빠름 | 중간 | 느림 |
| 권장 프로젝트 규모 | 소형 | 중형 | 대형 |

---

## 💡 결론

### 현재 상황에는 **옵션 1 (경량 구조)** 추천

**이유:**
1. 현재 Game Manager 구조가 이미 좋음
2. GameManager 아래에 UnitManager 추가하면 최소 변경으로 최대 효과
3. 복잡도를 높이지 않으면서도 관리 용이성 증가

**구현 예시:**
```csharp
public class UnitManager
{
    private List<Unit> playerUnits;
    private List<Unit> enemyUnits;
    
    public Unit GetNearestUnit(Unit source, List<Unit> targets)
    public void UpdateUnitHealth(Unit unit, int damage)
    public Unit GetUnitById(string unitId)
    public List<Unit> GetAliveUnits(List<Unit> units)
}
```

그러면 지금 바로 **UnitManager** 클래스를 만들어볼까요? 🚀


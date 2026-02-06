# 유닛 관리 시스템 가이드

## 개요
유닛 정보는 이제 **GameManager**에서 중앙 집중식으로 관리됩니다. 이를 통해 씬 전환 후에도 플레이어 유닛의 정보가 유지됩니다.

## 구조도
```
GameManager (씬 전환해도 유지)
├── PlayerUnits (항상 유지)
└── EnemyUnits (전투 중에만 관리, 전투 종료 후 초기화)
    └── CombatManager (전투 중에만 사용)
```

## 사용 방법

### 1. 전투 시작 전 (메인 씬 또는 맵 씬)
```csharp
// 플레이어 유닛 설정 (최초 1회)
List<Unit> playerUnits = GetPlayerUnitsFromScene(); // 또는 데이터베이스에서
GameManager.Instance.SetPlayerUnits(playerUnits);
```

### 2. 전투 씬 진입 시
```csharp
// 전투 씬이 로드될 때, 적 유닛 설정
List<Unit> enemyUnits = GetEnemyUnitsForBattle(); // 전투에 참여할 적 유닛
GameManager.Instance.SetEnemyUnits(enemyUnits);

// CombatManager 초기화
CombatManager.Instance.InitializeCombat(
    GameManager.Instance.GetPlayerUnits(),
    GameManager.Instance.GetEnemyUnits()
);
```

### 3. 전투 종료 후 (메인 씬으로 돌아갈 때)
```csharp
// 적 유닛 정보만 초기화
GameManager.Instance.ClearEnemyUnits();

// 플레이어 유닛은 상태가 유지됨 (체력, 상태이상 등)
List<Unit> survivalPlayers = GameManager.Instance.GetPlayerUnits();
```

## 주요 메서드

### GameManager
```csharp
// 플레이어 유닛 설정
void SetPlayerUnits(List<Unit> units)

// 적 유닛 설정
void SetEnemyUnits(List<Unit> units)

// 플레이어 유닛 조회
List<Unit> GetPlayerUnits()

// 적 유닛 조회
List<Unit> GetEnemyUnits()

// 전투 종료 후 적 유닛 초기화
void ClearEnemyUnits()
```

### CombatManager
```csharp
// GameManager에서 전달받은 유닛으로 전투 초기화
void InitializeCombat(List<Unit> playerUnits, List<Unit> enemyUnits)
```

## 데이터 흐름

### 전투 진행
```
메인 씬
  ↓
GameManager.SetPlayerUnits(플레이어들)
  ↓
[전투 씬 진입]
  ↓
GameManager.SetEnemyUnits(적들)
CombatManager.InitializeCombat(...)
  ↓
[전투 진행 중]
  ↓
[전투 종료]
  ↓
GameManager.ClearEnemyUnits()
  ↓
[메인 씬으로 돌아감]
  ↓
플레이어 유닛들의 상태가 유지됨!
```

## 장점
1. ✅ **데이터 보존**: 전투 후 플레이어 유닛 정보 유지
2. ✅ **중앙 관리**: 모든 매니저에서 GameManager를 통해 유닛 접근
3. ✅ **씬 독립성**: 어느 씬에서든 유닛 정보 접근 가능
4. ✅ **유연성**: 언제든 enemy를 변경하면서 player 유닛은 유지

## 주의사항
- 플레이어 유닛은 게임 시작 시 **한 번만** SetPlayerUnits으로 설정
- 전투마다 enemy는 SetEnemyUnits으로 새로 설정
- 전투 종료 시 반드시 ClearEnemyUnits()로 정리


# Unity 설정 가이드 - UnitUI 컴포넌트 추가

## 🎮 Unity에서 해야 할 작업

옵션 1 구현이 완료되었습니다! 이제 Unity 에디터에서 몇 가지 설정만 하면 됩니다.

---

## 📋 체크리스트

### 1. UnitUI 컴포넌트 추가 (필수)

각 Unit 프리팹/GameObject에 **UnitUI 컴포넌트**를 추가해야 합니다.

#### 단계:
1. Unity 에디터 열기
2. Unit 프리팹 또는 Hierarchy에서 Unit 선택
3. Inspector에서 `Add Component` 클릭
4. "UnitUI" 검색 후 추가
5. UnitUI 컴포넌트의 필드 설정:

```
✅ Health Text (TextMeshProUGUI)
✅ Health Slider (Image)
✅ Name Text (TextMeshProUGUI)
✅ Attack Cool Time Image (Image)
✅ Player UI (GameObject)
✅ Battle Mode Icons (GameObject[])
```

---

### 2. 기존 Unit 컴포넌트에서 필드 제거된 항목 확인

다음 필드들이 Unit.cs에서 제거되었으므로, 이제 UnitUI로 이동되었습니다:

#### Unit.cs에서 제거됨 ❌
- `healthText`
- `healthSlider`
- `name`
- `attackCoolTimeImage`
- `playerUI`
- `battleMode[]`
- `charStatusUI`

#### UnitUI.cs로 이동됨 ✅
- 위 모든 필드가 UnitUI에 있습니다

---

### 3. Inspector 설정 예시

```
GameObject: Player Unit
├── Unit (Script)
│   ├── Player Unit: ✓ (체크)
│   ├── Unit Data: [YourUnitDataSO]
│   ├── Stats: [...]
│   ├── Tilemap: [Reference to Tilemap]
│   └── Move Speed: 5
│
└── UnitUI (Script) ← 새로 추가!
    ├── Health Text: [CharStatusUI/HealthText]
    ├── Health Slider: [CharStatusUI/HealthBar]
    ├── Name Text: [CharStatusUI/NameText]
    ├── Attack Cool Time Image: [CharStatusUI/CoolTime]
    ├── Player UI: [PlayerUI GameObject]
    └── Battle Mode Icons:
        ├── Element 0: [NormalIcon]
        ├── Element 1: [AttackIcon]
        └── Element 2: [DefenceIcon]
```

---

## 🔧 빠른 설정 방법

### 방법 1: 스크립트로 자동 설정 (추천)

다음 스크립트를 에디터에서 실행하면 자동으로 UnitUI를 추가합니다:

```csharp
// Assets/Editor/AddUnitUIToAll.cs
using UnityEditor;
using UnityEngine;

public class AddUnitUIToAll : EditorWindow
{
    [MenuItem("Tools/Add UnitUI to All Units")]
    static void AddUnitUIComponents()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        int count = 0;
        
        foreach (Unit unit in allUnits)
        {
            if (unit.GetComponent<UnitUI>() == null)
            {
                unit.gameObject.AddComponent<UnitUI>();
                count++;
            }
        }
        
        Debug.Log($"UnitUI 컴포넌트를 {count}개 유닛에 추가했습니다!");
    }
}
```

실행:
1. Unity 상단 메뉴 → `Tools` → `Add UnitUI to All Units` 클릭
2. 모든 Unit에 자동으로 UnitUI 추가됨

---

### 방법 2: 수동 설정

각 Unit GameObject/Prefab에서:
1. Inspector → `Add Component`
2. `UnitUI` 검색 후 추가
3. 드래그 앤 드롭으로 UI 요소 연결

---

## ⚠️ 주의사항

### 기존 프리팹이 있는 경우
1. **프리팹 업데이트 필요**: 기존 Unit 프리팹을 열어서 UnitUI 추가
2. **Apply to Prefab**: 변경사항을 프리팹에 적용
3. **씬의 모든 인스턴스 업데이트**: Prefab 변경 후 씬 재로드

### 컴파일 오류가 발생하면
```
1. Unity 에디터 재시작
2. Assets → Reimport All
3. Library 폴더 삭제 후 재시작
```

---

## ✅ 테스트 방법

### 1. Unit이 올바르게 작동하는지 확인
```csharp
// Play 모드에서 콘솔 확인
// Unit.cs Awake()에서 다음 경고가 없어야 함:
// "[Unit] {name}: UnitUI 컴포넌트가 없습니다"
```

### 2. UI가 업데이트되는지 확인
- 체력이 감소하면 체력바가 줄어드는지
- 공격 쿨타임이 표시되는지
- 전투 모드 아이콘이 변경되는지

### 3. GameManager 확인
```csharp
// Play 모드에서 테스트
void Start()
{
    var unitMgr = GameManager.Instance.UnitManager;
    Debug.Log($"플레이어 유닛: {unitMgr.GetPlayerUnits().Count}개");
    Debug.Log($"적 유닛: {unitMgr.GetEnemyUnits().Count}개");
}
```

---

## 🎯 완료 후 확인

- [ ] 모든 Unit에 UnitUI 컴포넌트 추가됨
- [ ] UnitUI의 모든 필드가 올바르게 설정됨
- [ ] 컴파일 오류 없음
- [ ] Play 모드에서 UI가 정상 작동
- [ ] 경고 메시지 없음

---

## 💡 문제 해결

### "UnitUI 컴포넌트가 없습니다" 경고
→ Unit GameObject에 UnitUI 컴포넌트를 추가하세요

### UI가 업데이트되지 않음
→ UnitUI의 필드가 올바르게 연결되었는지 확인

### NullReferenceException
→ Inspector에서 모든 필드가 할당되었는지 확인

---

## 📞 추가 도움말

더 자세한 내용은 다음 문서를 참고하세요:
- `UNIT_MANAGEMENT_GUIDE.md` - 사용 가이드
- `OPTION1_IMPLEMENTATION_COMPLETE.md` - 완료 내역
- `UNIT_STRUCTURE_IMPROVEMENT.md` - 개선 상세 설명

설정 완료 후 바로 사용할 수 있습니다! 🚀


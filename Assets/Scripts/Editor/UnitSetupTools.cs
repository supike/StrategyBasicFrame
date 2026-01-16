using UnityEditor;
using UnityEngine;

/// <summary>
/// Unit GameObject에 UnitUI 컴포넌트를 자동으로 추가하는 에디터 도구
/// 사용법: Unity 메뉴 → Tools → Unit Setup → Add UnitUI to All Units
/// </summary>
public class UnitSetupTools : EditorWindow
{
    [MenuItem("Tools/Unit Setup/Add UnitUI to All Units")]
    static void AddUnitUIToAllUnits()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        int addedCount = 0;
        int existingCount = 0;

        foreach (Unit unit in allUnits)
        {
            if (unit.GetComponent<UnitUI>() == null)
            {
                unit.gameObject.AddComponent<UnitUI>();
                EditorUtility.SetDirty(unit.gameObject);
                addedCount++;
                Debug.Log($"[UnitSetup] UnitUI 추가됨: {unit.gameObject.name}");
            }
            else
            {
                existingCount++;
            }
        }

        if (addedCount > 0)
        {
            Debug.Log($"<color=green>[UnitSetup] 완료! {addedCount}개의 Unit에 UnitUI를 추가했습니다.</color>");
        }
        
        if (existingCount > 0)
        {
            Debug.Log($"[UnitSetup] {existingCount}개의 Unit은 이미 UnitUI를 가지고 있습니다.");
        }

        if (addedCount == 0 && existingCount == 0)
        {
            Debug.LogWarning("[UnitSetup] 씬에 Unit 컴포넌트를 찾을 수 없습니다.");
        }
    }

    [MenuItem("Tools/Unit Setup/Validate All UnitUI")]
    static void ValidateAllUnitUI()
    {
        UnitUI[] allUnitUIs = FindObjectsOfType<UnitUI>();
        int validCount = 0;
        int invalidCount = 0;

        foreach (UnitUI unitUI in allUnitUIs)
        {
            if (unitUI.ValidateUI())
            {
                validCount++;
            }
            else
            {
                invalidCount++;
                Debug.LogWarning($"[UnitSetup] UI 설정 불완전: {unitUI.gameObject.name}", unitUI.gameObject);
            }
        }

        Debug.Log($"[UnitSetup] 검증 완료 - 정상: {validCount}개, 불완전: {invalidCount}개");
    }

    [MenuItem("Tools/Unit Setup/Remove All UnitUI (Cleanup)")]
    static void RemoveAllUnitUI()
    {
        if (!EditorUtility.DisplayDialog(
            "UnitUI 제거",
            "정말로 모든 Unit에서 UnitUI 컴포넌트를 제거하시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
            "제거",
            "취소"))
        {
            return;
        }

        UnitUI[] allUnitUIs = FindObjectsOfType<UnitUI>();
        int removeCount = 0;

        foreach (UnitUI unitUI in allUnitUIs)
        {
            DestroyImmediate(unitUI);
            removeCount++;
        }

        Debug.Log($"<color=yellow>[UnitSetup] {removeCount}개의 UnitUI 컴포넌트를 제거했습니다.</color>");
    }

    [MenuItem("Tools/Unit Setup/Report Unit Status")]
    static void ReportUnitStatus()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        
        Debug.Log("=== Unit 상태 리포트 ===");
        Debug.Log($"총 Unit 개수: {allUnits.Length}");
        
        int playerUnits = 0;
        int enemyUnits = 0;
        int withUI = 0;
        int withoutUI = 0;

        foreach (Unit unit in allUnits)
        {
            if (unit.playerUnit)
                playerUnits++;
            else
                enemyUnits++;

            if (unit.GetComponent<UnitUI>() != null)
                withUI++;
            else
                withoutUI++;
        }

        Debug.Log($"플레이어 유닛: {playerUnits}개");
        Debug.Log($"적 유닛: {enemyUnits}개");
        Debug.Log($"UnitUI 있음: {withUI}개");
        Debug.Log($"UnitUI 없음: {withoutUI}개");
        
        if (withoutUI > 0)
        {
            Debug.LogWarning($"⚠️ {withoutUI}개의 Unit에 UnitUI가 없습니다. 'Add UnitUI to All Units'를 실행하세요.");
        }
        else
        {
            Debug.Log("<color=green>✓ 모든 Unit에 UnitUI가 설정되어 있습니다!</color>");
        }
        
        Debug.Log("====================");
    }
}


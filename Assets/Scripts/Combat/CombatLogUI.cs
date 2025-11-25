namespace Combat
{
public class CombatLogUI : MonoBehaviour
{
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private Transform logContainer;
    [SerializeField] private int maxVisibleEntries = 10;
    
    private Queue<GameObject> logEntries = new Queue<GameObject>();
    
    void Start()
    {
        // 이벤트 구독
        CombatEventSystem.Instance.OnCombatEvent += OnCombatEventReceived;
    }
    
    void OnDestroy()
    {
        if (CombatEventSystem.Instance != null)
        {
            CombatEventSystem.Instance.OnCombatEvent -= OnCombatEventReceived;
        }
    }
    
    void OnCombatEventReceived(CombatEvent combatEvent)
    {
        // 특정 이벤트만 UI에 표시
        if (ShouldDisplayEvent(combatEvent.eventType))
        {
            AddLogEntry(combatEvent);
        }
    }
    
    void AddLogEntry(CombatEvent combatEvent)
    {
        GameObject entry = Instantiate(logEntryPrefab, logContainer);
        TextMeshProUGUI text = entry.GetComponent<TextMeshProUGUI>();
        
        // 이벤트 타입에 따른 색상 지정
        Color textColor = GetEventColor(combatEvent.eventType);
        text.color = textColor;
        text.text = combatEvent.message;
        
        logEntries.Enqueue(entry);
        
        // 최대 표시 개수 초과 시 제거
        if (logEntries.Count > maxVisibleEntries)
        {
            GameObject oldEntry = logEntries.Dequeue();
            Destroy(oldEntry);
        }
    }
    
    bool ShouldDisplayEvent(CombatEventType eventType)
    {
        // 중요한 이벤트만 표시
        switch (eventType)
        {
            case CombatEventType.DamageDealt:
            case CombatEventType.CriticalHit:
            case CombatEventType.AttackDodged:
            case CombatEventType.UnitDefeated:
            case CombatEventType.StatusEffectApplied:
                return true;
            default:
                return false;
        }
    }
    
    Color GetEventColor(CombatEventType eventType)
    {
        switch (eventType)
        {
            case CombatEventType.DamageDealt:
                return Color.white;
            case CombatEventType.CriticalHit:
                return new Color(1f, 0.5f, 0f); // 주황색
            case CombatEventType.AttackDodged:
                return Color.cyan;
            case CombatEventType.UnitDefeated:
                return Color.red;
            case CombatEventType.HealingReceived:
                return Color.green;
            default:
                return Color.gray;
        }
    }
}
}
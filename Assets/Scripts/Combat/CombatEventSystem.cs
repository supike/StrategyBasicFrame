using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public enum CombatEventType
    {
        TurnStarted,
        TurnEnded,
        UnitSelected,
        MovementStarted,
        MovementCompleted,
        AttackStarted,
        AttackDodged,
        AttackBlocked,
        DamageDealt,
        CriticalHit,
        CounterAttackStarted,
        CounterAttackHit,
        HealingReceived,
        StatusEffectApplied,
        StatusEffectExpired,
        UnitDefeated,
        AbilityUsed,
        ItemUsed
    }

    [System.Serializable]
    public class CombatEvent
    {
        public CombatEventType eventType;
        public Unit attacker;
        public Unit defender;
        public int damage;
        public string message;
        public float timestamp;
    
        public CombatEvent()
        {
            timestamp = Time.time;
        }
    }

    public class CombatEventSystem: MonoBehaviour
    {
        public static CombatEventSystem Instance { get; private set; }
    
        // 이벤트 델리게이트
        public System.Action<CombatEvent> OnCombatEvent;
    
        // 전투 로그 저장
        private Queue<CombatEvent> combatLog = new Queue<CombatEvent>();
        private const int MAX_LOG_SIZE = 100;
    
        void Awake()
        {
            Instance = this;
        }
    
        public void TriggerEvent(CombatEvent combatEvent)
        {
            // 이벤트 로그에 추가
            combatLog.Enqueue(combatEvent);
        
            if (combatLog.Count > MAX_LOG_SIZE)
            {
                combatLog.Dequeue(); // 오래된 로그 제거
            }
        
            // 구독자들에게 알림
            OnCombatEvent?.Invoke(combatEvent);
        
            // 콘솔 로그
            Debug.Log($"[{combatEvent.eventType}] {combatEvent.message}");
        }
    
        // public List<CombatEvent> GetRecentEvents(int count)
        // {
        //     return combatLog.TakeLast(count).ToList();
        //     
        // }
    
        public void ClearLog()
        {
            combatLog.Clear();
        }
    }
}
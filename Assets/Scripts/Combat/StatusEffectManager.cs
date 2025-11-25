using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
[System.Serializable]
public class StatusEffect
{
    public enum StatusType
    {
        Stun,       // 기절 (행동 불가)
        Slow,       // 둔화 (이동력 -50%)
        Poison,     // 독 (턴마다 데미지)
        Burn,       // 화상 (지속 데미지)
        Blind,      // 실명 (명중률 감소)
        Berserk,    // 광폭화 (공격력 증가, 방어력 감소)
        Shield      // 보호막
    }
    
    public StatusType type;
    public int duration;        // 지속 턴 수
    public int power;           // 효과 강도
    public Unit afflictedUnit;
}

public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    
    public void ApplyStatusEffect(Unit unit, StatusEffect.StatusType type, int duration, int power)
    {
        // 기존 효과 중복 확인
        StatusEffect existing = activeEffects.Find(e => 
            e.afflictedUnit == unit && e.type == type
        );
        
        if (existing != null)
        {
            // 지속시간 갱신 또는 중첩
            existing.duration = Mathf.Max(existing.duration, duration);
            existing.power += power;
        }
        else
        {
            // 새 효과 추가
            StatusEffect newEffect = new StatusEffect
            {
                type = type,
                duration = duration,
                power = power,
                afflictedUnit = unit
            };
            
            activeEffects.Add(newEffect);
            OnStatusEffectApplied(unit, type);
        }
        
        CombatEventSystem.Instance.TriggerEvent(new CombatEvent
        {
            eventType = CombatEventType.StatusEffectApplied,
            defender = unit,
            message = $"{unit.name} is affected by {type}!"
        });
    }
    
    public void ProcessStatusEffects(Unit unit)
    {
        List<StatusEffect> unitEffects = activeEffects.FindAll(e => e.afflictedUnit == unit);
        
        foreach (StatusEffect effect in unitEffects)
        {
            switch (effect.type)
            {
                case StatusEffect.StatusType.Poison:
                    unit.TakeDamage(effect.power, DamageType.Poison);
                    break;
                    
                case StatusEffect.StatusType.Burn:
                    unit.TakeDamage(effect.power * 2, DamageType.Magical);
                    break;
            }
            
            // 지속시간 감소
            effect.duration--;
            
            if (effect.duration <= 0)
            {
                RemoveStatusEffect(effect);
            }
        }
    }
    
    void OnStatusEffectApplied(Unit unit, StatusEffect.StatusType type)
    {
        switch (type)
        {
            case StatusEffect.StatusType.Stun:
                unit.IsStunned = true;
                break;
                
            case StatusEffect.StatusType.Slow:
                unit.IsSlowed = true;
                unit.stats.movement = Mathf.Max(1, unit.stats.movement / 2);
                break;
        }
    }
    
    void RemoveStatusEffect(StatusEffect effect)
    {
        activeEffects.Remove(effect);
        
        CombatEventSystem.Instance.TriggerEvent(new CombatEvent
        {
            eventType = CombatEventType.StatusEffectExpired,
            defender = effect.afflictedUnit,
            message = $"{effect.afflictedUnit.name}'s {effect.type} wore off."
        });
    }
}
}
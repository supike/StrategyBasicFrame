using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// GameEvent: ScriptableObject 기반 이벤트
[CreateAssetMenu(fileName = "GameEvent", menuName = "Game/Event")]
public class GameEventSO : ScriptableObject
{
    private List<UnityAction> listeners = new List<UnityAction>();
    
    public void Raise()
    {
        for(int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].Invoke();
    }
    
    public void RegisterListener(UnityAction listener)
    {
        listeners.Add(listener);
    }
    
    public void UnregisterListener(UnityAction listener)
    {
        listeners.Remove(listener);
    }
}
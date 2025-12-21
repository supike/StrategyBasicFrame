using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum UnitClass
{
    Fighter,      // 근접 DPS
    Archer,       // 원거리 DPS
    Cavalry,      // 기동형
    Defender,     // 탱커
    Skirmisher,   // 유연형
    Spiritualist  // 마법/지원
}

public enum UnitType
{
    Infantry,  // 보병
    Ranged,    // 원거리
    Mounted,   // 기마
    Special    // 특수
}

/// <summary>
/// Model: 게임 데이터 저장.
/// 유닛의 기본 데이터와 스탯을 정의하는 ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "UnitData", menuName = "Game/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName;
    public UnitClass unitClass;
    public UnitType unitType;
    public Sprite icon;
    //public GameObject prefab;
    
    [Header("Combat Stats")]
    public int maxHealth;
    public int attack;
    public int defense;
    public int magicDefense;
    
    [Header("Movement & Range")]
    public int movementRange;      // 이동 가능 타일 수
    public int attackRange;        // 공격 사거리
    public int visionRange;        // 시야 범위
    
    [Header("Action System")]
    public int maxActionPoints = 2; // 턴당 액션 포인트
    public int moveCost = 1;        // 이동 비용
    public int attackCost = 1;      // 공격 비용
    
    [Header("Special Attributes")]
    public float criticalChance = 0.05f;    // 치명타 확률 (5%)
    public float criticalMultiplier = 1.5f; // 치명타 배율
    public float dodgeChance = 0.1f;        // 회피 확률
    public int initiative;                   // 행동 순서 결정
    
    [Header("Status Resistances")]
    public float stunResist = 0f;
    public float slowResist = 0f;
    public float poisonResist = 0f;
    
    [Header("Terrain Modifiers")]
    public float forestMovementPenalty = 0.5f;  // 숲 이동 페널티
    public float mountainMovementPenalty = 2f;   // 산악 이동 페널티
    
    [Header("Production Cost")]
    public List<ResourceAmount> recruitmentCost;
    public float productionTime = 5f;
    
    //[Header("Special Abilities")]
    //public List<AbilitySO> abilities;
    
    [Header("Description")]
    [TextArea(3, 10)]
    public string description;
}
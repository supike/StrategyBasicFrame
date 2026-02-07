using System.Collections.Generic;
using UnityEngine;


public enum EClass
{
    Infantry,  // 보병
    Ranged,    // 원거리
    Mounted,   // 기마
    Special    // 특수
}

public enum EmotionCondition
{
    desperatated,
    bad,
    normal,
    good,
    happy,
}

public enum HealthCondition
{
    woond,
    normal,
    good,
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public EClass unitclass;
    public Sprite portraitImage;
    public Sprite iconImage;
    public int ages;
    public int reputation;
    public HealthCondition HealthCondition = HealthCondition.normal;
    public EmotionCondition EmotionCondition = EmotionCondition.normal;

    [Header("Description")]
    [TextArea(3, 10)]
    public string description;

    [Header("Default Stats")]
    public int stamina = 10;
    public int speed = 10;
    public int balance = 10;
    public int strength = 10;

    public int intel = 10;
    public int health = 10;
    public int luck = 10;

    [Header("Core Traits (0~1)")]
    public float aggression;      // 공격성
    public float bravery;         // 공포 저항
    public float discipline;      // 명령 순응
    public float empathy;         // 아군 반응
    public float impulsiveness;   // 충동성

    [Header("Behavior Bias")]
    public float retreatThreshold;  // 후퇴 판단 기준
    public float allyDeathImpact;   // 아군 사망 영향도

    [Header("Flavor")]
    public string backgroundTag;

}
public class UnitStateRuntime
{
    public int currentHP;

    [Header("Emotions (0~100)")]
    public float fear;
    public float anger;
    public float confidence;
    public float stress;

    [Header("Flags")]
    public bool isPanicked;
    public bool isTraumatized;
}

public enum MemoryTag
{
    WitnessedMassacre,
    SuccessfulRetreat,
    AbandonedByCommander,
    SoloVictory
}
[CreateAssetMenu(menuName = "Unit/Memory")]
public class UnitMemorySO : ScriptableObject
{
    [Header("Experience Counters")]
    public int battlesSurvived;
    public int retreats;
    public int alliesDiedNearby;

    [Header("Behavior Tags")]
    public List<MemoryTag> memoryTags;

    [Header("Long-term Modifiers")]
    public float braveryModifier;
    public float aggressionModifier;
}

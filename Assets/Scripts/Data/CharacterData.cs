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
    public int stamina=10;
    public int speed=10;
    public int balance=10;
    public int strength=10;
    
    public int agression=10;
    public int bravery=10;
    public int sight=10;
    
    public int intel=10;
    public int health=10;
    public int luck=10;

}

//To-do : CharacterData를 상속받아 현재 체력만 추가한 클래스. 추후에 상태이상 등도 추가할 예정

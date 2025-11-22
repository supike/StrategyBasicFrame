using UnityEngine;


public enum EClass
{
    Knight, Ranger, Wizard
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public bool enable;
    public string CharacterName;
    public EClass Class;
    public Sprite PortraitImage;
    public int power;
    public int intel;
    public int health;
    public int luck;
    public int curHealth;

    public CharacterData()
    {
        curHealth = health;
    }
    
    public void setCurHealthReset(int _health)
    {
        curHealth += _health;
    }

}

//To-do : CharacterData를 상속받아 현재 체력만 추가한 클래스. 추후에 상태이상 등도 추가할 예정

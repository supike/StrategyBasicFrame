using Combat;
using UnityEngine;
using UnityEngine.UI;

public class PanelCastle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ButtonClick1()
    {
        CombatManager.Instance.AttackMode();
    }
    public void ButtonClick2()
    {
        CombatManager.Instance.DefenceMode();
    }
    public void ButtonClick3()
    {
        Debug.Log("Button Clicked3");
    }
}

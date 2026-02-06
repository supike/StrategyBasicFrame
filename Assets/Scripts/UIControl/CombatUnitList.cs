using System.Collections.Generic;
using Combat;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CombatUnitList : MonoBehaviour
{
    public CombatManager combatManager;

    [SerializeField]
    public Image[] characterImage;


    public List<Unit> GetPlayerUnits()
    {
        return combatManager.GetAllPlayerUnits();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < combatManager.GetAllPlayerUnits().Count; i++)
        {

            characterImage[i].sprite = combatManager.GetAllPlayerUnits()[i].unitData.iconImage;
        }

    }
}

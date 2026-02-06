using UnityEngine;

public class AttributesManager : MonoBehaviour
{
    public int health;

    public int attack;

    public void TakeDamage(int amout)
    {
        health -= amout;
        DamagePopUpGenerator.Instance.CreateDamagePopUp(transform.position, amout.ToString());
    }

    public void DealDamage(GameObject target)
    {
        var atm = target.GetComponent<AttributesManager>();
        if (atm != null)
        {
            atm.TakeDamage(attack);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using Combat;
using UnityEngine;
using TMPro;

// https://www.youtube.com/watch?v=KOt85IoD__4 유튜브 참고함.
public class DamagePopUpGenerator : MonoBehaviour
{
    public static DamagePopUpGenerator Instance;
    public GameObject damagePopUpPrefab;
    
    // public enum DamageType
    // {
    //     Normal,
    //     Critical,
    //     Heal
    // }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         CreateDamagePopUp(transform.position, Random.Range(0, 1000).ToString());
    //     }
    // }
    
    public void CreateDamagePopUp(Vector3 position, string text, DamageType damageType = DamageType.Physical)
    {
        if (damagePopUpPrefab != null)
        {
            position.y += 2.0f; // 팝업이 캐릭터와 겹치지 않도록 약간 위로 올리기
            GameObject popUp = Instantiate(damagePopUpPrefab, position, Quaternion.identity);
            DamagePopUpAnimation animation = popUp.GetComponent<DamagePopUpAnimation>();
            animation.typePopup = damageType;
            
            popUp.SetActive(true);
            TextMeshProUGUI popUpScript = popUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            popUpScript.transform.gameObject.SetActive(true);
            popUpScript.text = text;
            if (popUpScript != null)
            {
                //popUpScript.SetDamage(damageAmount);
            }
            Destroy(popUp, 1.0f); // 1초 후에 팝업 제거
        }
        else
        {
            Debug.LogWarning("DamagePopUpPrefab이 할당되지 않았습니다.");
        }
    }
}

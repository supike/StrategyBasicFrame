using System;
using Combat;
using TMPro;
using UnityEngine;

public class DamagePopUpAnimation : MonoBehaviour
{
    public AnimationCurve animationCurve;   
    public AnimationCurve animationCurveHeight;   
    public AnimationCurve animationCurveScale;   
    private TextMeshProUGUI text;
    private float time = 0;
    private Vector3 originPosition;
    private Color[] typeColor;
    
    // Update에서 재사용할 변수들
    private Color tempColor;
    private Vector3 tempPosition;
    public DamageType typePopup = DamageType.Physical;

    void Awake()
    {
        text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        originPosition = transform.position;
        typeColor = new Color[]
        {
            new Color(1, 1, 1), // 일반
            new Color(0, 1, 0), // 치유
            new Color(1, 0, 0), // 치명타
            new Color(1, 0.5f, 0), // 막기
        };
        
        // 초기화
        tempColor = Color.white;
        tempPosition = Vector3.zero;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Color 재사용 (typeColor[1]의 값을 사용, 주의: 마지막 r은 b의 오타로 보입니다)
        tempColor.r = typeColor[(int)typePopup].r;
        tempColor.g = typeColor[(int)typePopup].g;
        tempColor.b = typeColor[(int)typePopup].b; // 원래 코드에서 r로 되어있던 부분 수정
        tempColor.a = animationCurve.Evaluate(time);
        text.color = tempColor;
        
        // Scale 설정 (Vector3.one을 곱하는 대신 직접 설정)
        float scaleValue = animationCurveScale.Evaluate(time);
        transform.localScale = Vector3.one * scaleValue; // Vector3.one은 static 변수이므로 할당 없음
        
        // Position 재사용
        tempPosition.x = originPosition.x;
        tempPosition.y = originPosition.y + animationCurveHeight.Evaluate(time);
        tempPosition.z = originPosition.z;
        transform.position = tempPosition;
        
        time += Time.deltaTime;
    }
}

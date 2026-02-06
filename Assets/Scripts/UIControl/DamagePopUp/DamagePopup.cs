using UnityEngine;
using TMPro;

namespace BattleYouCan{

    public class DamagePopup : MonoBehaviour
    {
        private TMP_Text damageText;
        [SerializeField] private float lifetime = 1.0f;
        [SerializeField] private float moveSpeed = 1.0f;
        [SerializeField] private float fadeSpeed = 3.0f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0);
        [SerializeField] private Color criticalHitColor = Color.red;

        private float timeAlive = 0f;
        private Color textColor;

        private void Awake()
        {
            if (damageText == null)
                damageText = GetComponent<TMP_Text>();
            
            textColor = damageText.color;
        }

        public void Setup(int damageAmount, bool isCritical = false)
        {
            damageText.text = damageAmount.ToString();
            
            // 크리티컬 히트의 경우 색상 변경 및 크기 확대
            if (isCritical)
            {
                damageText.color = criticalHitColor;
                damageText.fontSize *= 1.2f;
            }
        }

        private void Update()
        {
            // 위로 올라가는 효과
            transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
            
            // 시간 업데이트
            timeAlive += Time.deltaTime;
            
            // 수명의 절반 이후부터 페이드 아웃 효과
            if (timeAlive > lifetime * 0.5f)
            {
                // 알파값 감소
                textColor.a = Mathf.Lerp(textColor.a, 0, fadeSpeed * Time.deltaTime);
                damageText.color = textColor;
                
                // 완전히 투명해지면 오브젝트 파괴
                if (textColor.a < 0.05f)
                {
                    Destroy(gameObject);
                }
            }
            
            // 수명이 다 되면 파괴
            if (timeAlive >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        // 데미지 텍스트 생성 메서드 (정적 메서드로 쉽게 호출 가능)
        public static DamagePopup Create(Vector3 position, int damageAmount, bool isCritical = false)
        {
            // Resources 폴더에서 프리팹 로드 (미리 만들어 놓아야 함)
            Transform damagePopupTransform = Instantiate(Resources.Load<Transform>("DamagePopup"));
            
            // 위치 설정 (약간 위로 오프셋)
            damagePopupTransform.position = position;
            
            // 컴포넌트 가져오기
            DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
            
            // 데미지 값 설정
            damagePopup.Setup(damageAmount, isCritical);
            
            return damagePopup;
        }
    }
}
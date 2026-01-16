using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Unit의 UI 표시를 담당하는 컴포넌트
/// Unit 클래스에서 UI 로직을 분리하여 책임을 명확히 합니다.
/// </summary>
public class UnitUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthSlider;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image attackCoolTimeImage;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject[] battleModeIcons;
    
    private GameObject charStatusUI;
    private Unit unit;

    private void Awake()
    {
        charStatusUI = transform.Find("CharStatusUI")?.gameObject;
        unit = GetComponent<Unit>();
    }

    /// <summary>
    /// 유닛 UI를 초기화합니다
    /// </summary>
    public void Initialize(string unitName, Sprite icon)
    {
        if (nameText != null)
            nameText.text = unitName;

        if (playerUI != null)
        {
            SpriteRenderer sr = playerUI.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.sprite = icon;
        }

        if (battleModeIcons != null && battleModeIcons.Length > 0)
        {
            battleModeIcons[0]?.SetActive(true);
        }
    }

    /// <summary>
    /// 체력 UI를 업데이트합니다
    /// </summary>
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}";
        }

        if (healthSlider != null)
        {
            healthSlider.fillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        }
    }

    /// <summary>
    /// 공격 쿨타임 UI를 업데이트합니다
    /// </summary>
    public void UpdateCoolTimeUI(float fillAmount)
    {
        if (attackCoolTimeImage != null)
        {
            attackCoolTimeImage.fillAmount = fillAmount;
        }
    }

    /// <summary>
    /// 전투 모드 아이콘을 설정합니다
    /// </summary>
    public void SetBattleModeIcon(UnitMode mode)
    {
        if (battleModeIcons == null || battleModeIcons.Length == 0)
            return;

        // 모든 아이콘 비활성화
        foreach (var icon in battleModeIcons)
        {
            if (icon != null)
                icon.SetActive(false);
        }

        // 해당 모드 아이콘 활성화
        int modeIndex = (int)mode;
        if (modeIndex >= 0 && modeIndex < battleModeIcons.Length && battleModeIcons[modeIndex] != null)
        {
            battleModeIcons[modeIndex].SetActive(true);
        }
    }

    /// <summary>
    /// 캐릭터 상태 UI의 방향을 설정합니다
    /// </summary>
    public void SetUIDirection(int direction)
    {
        if (charStatusUI != null)
        {
            Vector3 scale = charStatusUI.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            charStatusUI.transform.localScale = scale;
        }
    }

    /// <summary>
    /// UI를 표시하거나 숨깁니다
    /// </summary>
    public void SetUIVisible(bool visible)
    {
        if (charStatusUI != null)
        {
            charStatusUI.SetActive(visible);
        }
    }

    /// <summary>
    /// 모든 UI 요소가 올바르게 설정되었는지 확인
    /// </summary>
    public bool ValidateUI()
    {
        bool isValid = true;

        if (healthText == null)
        {
            Debug.LogWarning($"[UnitUI] {gameObject.name}: healthText가 설정되지 않았습니다.");
            isValid = false;
        }

        if (healthSlider == null)
        {
            Debug.LogWarning($"[UnitUI] {gameObject.name}: healthSlider가 설정되지 않았습니다.");
            isValid = false;
        }

        return isValid;
    }
}


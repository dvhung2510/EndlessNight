using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("UI Components")]
    public Image frameBar;    // Khung ngoài của thanh máu (Image)
    public Image fillBar;     // Phần fill của thanh máu (Image)
    public TextMeshProUGUI healthText; // Text hiển thị số máu

    [Header("Màu sắc")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f; // Ngưỡng dưới 30% máu sẽ chuyển sang màu đỏ

    [Header("Animation")]
    public bool useAnimations = true;
    public float animationSpeed = 5f;
    private float targetFillAmount;

    private void Awake()
    {
        // Tự động tìm components nếu chưa gán
        if (frameBar == null)
        {
            frameBar = transform.Find("FrameBar")?.GetComponent<Image>();
        }

        if (fillBar == null)
        {
            fillBar = transform.Find("FillBar")?.GetComponent<Image>();
        }

        if (healthText == null)
        {
            healthText = transform.Find("Health Text")?.GetComponent<TextMeshProUGUI>();
        }
    }

    // Phương thức chính để cập nhật thanh máu
    public void UpdateBar(int currentValue, int maxValue)
    {
        float normalizedValue = (float)currentValue / maxValue;

        if (useAnimations)
        {
            targetFillAmount = normalizedValue;
            StartCoroutine(AnimateBar());
        }
        else
        {
            UpdateFillAmount(normalizedValue);
        }

        // Cập nhật text hiển thị máu
        if (healthText != null)
        {
            healthText.text = currentValue + " / " + maxValue;
        }
    }

    private System.Collections.IEnumerator AnimateBar()
    {
        if (fillBar != null)
        {
            float currentFill = fillBar.fillAmount;

            while (Mathf.Abs(currentFill - targetFillAmount) > 0.01f)
            {
                currentFill = Mathf.Lerp(currentFill, targetFillAmount, Time.deltaTime * animationSpeed);
                UpdateFillAmount(currentFill);
                yield return null;
            }

            UpdateFillAmount(targetFillAmount);
        }
    }

    private void UpdateFillAmount(float fillAmount)
    {
        // Cập nhật giá trị thanh máu bằng cách điều chỉnh fillAmount của Image
        if (fillBar != null)
        {
            fillBar.fillAmount = fillAmount;

            // Cập nhật màu sắc dựa trên lượng máu
            fillBar.color = Color.Lerp(lowHealthColor, fullHealthColor,
                fillAmount < lowHealthThreshold ? 0 : (fillAmount - lowHealthThreshold) / (1 - lowHealthThreshold));
        }
    }
}
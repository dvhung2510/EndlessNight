using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class HealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 1000;
    public int currentHealth;

    [Header("UI Elements")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent onDamaged;
    public UnityEvent onHealed;

    private void Start()
    {
        // Khởi tạo máu khi bắt đầu
        currentHealth = maxHealth;
        UpdateUI();
    }

    // Gây sát thương cho nhân vật
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damageAmount;

        // Giới hạn giá trị tối thiểu
        if (currentHealth < 0)
            currentHealth = 0;

        // Cập nhật UI
        UpdateUI();

        // Gọi event khi bị damage
        onDamaged?.Invoke();

        // Kiểm tra xem nhân vật có chết không
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Hồi máu cho nhân vật
    public void HealHealth(int healAmount)
    {
        // Hồi máu
        currentHealth += healAmount;

        // Giới hạn giá trị tối đa
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        // Cập nhật UI
        UpdateUI();

        // Gọi event khi được hồi máu
        onHealed?.Invoke();
    }

    // Xử lý khi nhân vật chết
    private void Die()
    {
        // Gọi event khi chết
        onDeath?.Invoke();

        Debug.Log("Nhân vật đã chết!");
    }

    // Cập nhật UI hiển thị máu
    private void UpdateUI()
    {
        // Cập nhật thanh máu
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Cập nhật text hiển thị máu
        if (healthText != null)
        {
            healthText.text = currentHealth + "/" + maxHealth;
        }
    }

    // Đặt lại máu về tối đa
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }
}
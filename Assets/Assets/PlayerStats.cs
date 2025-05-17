using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Basic Info")]
    public string playerName = "Nhân vật";
    public int level = 1;
    public string playerClass = "Chiến binh";
    public Sprite avatar;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("Combat Stats")]
    public float attack = 10f;
    public float defense = 5f;
    public float speed = 7f;
    public float criticalChance = 5f; // Phần trăm

    [Header("Experience")]
    public int currentExp = 0;
    public int expToNextLevel = 100;

    // Phương thức để tăng cấp
    public void LevelUp()
    {
        level++;
        maxHealth += 10;
        currentHealth = maxHealth;
        attack += 2f;
        defense += 1f;
        speed += 0.5f;
        criticalChance += 0.5f;

        // Tính toán exp cần cho cấp tiếp theo
        expToNextLevel = level * 100;

        // Thông báo level up nếu cần
        Debug.Log("Level Up! Nhân vật đã đạt cấp " + level);
    }

    // Phương thức nhận exp
    public void GainExp(int amount)
    {
        currentExp += amount;

        // Kiểm tra nếu đủ exp để lên cấp
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
    }

    // Phương thức nhận sát thương
    public void TakeDamage(int damage)
    {
        // Tính toán sát thương thực tế dựa trên phòng thủ
        float damageReduction = defense / (defense + 50f); // Công thức giảm sát thương
        int actualDamage = Mathf.Max(1, Mathf.RoundToInt(damage * (1f - damageReduction)));

        currentHealth -= actualDamage;

        // Đảm bảo HP không âm
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log("Nhân vật nhận " + actualDamage + " sát thương! HP còn lại: " + currentHealth);

        // Kiểm tra xem nhân vật còn sống không
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Phương thức hồi máu
    public void Heal(int amount)
    {
        currentHealth += amount;

        // Đảm bảo HP không vượt quá tối đa
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log("Nhân vật hồi " + amount + " HP! HP hiện tại: " + currentHealth);
    }

    // Phương thức khi nhân vật chết
    void Die()
    {
        Debug.Log("Nhân vật đã ngã xuống!");
        // Thêm logic xử lý khi nhân vật chết
    }
}
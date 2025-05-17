using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyDamageConfig
{
    public string enemyName;
    public int damage;
}

public class PlayerHealth : MonoBehaviour
{
    [Header("Thông số máu")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    [SerializeField] private GameObject healthBarObject; // Thêm tham chiếu để kéo trực tiếp trong Inspector
    private HealthBar healthBarScript;

    [Header("Hiệu ứng")]
    public float invincibleTime = 1f;
    private bool isInvincible = false;
    private bool isDead = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Cấu hình sát thương Enemy")]
    public LayerMask enemyLayer;
    public int defaultEnemyDamage = 20;
    public bool useConfigFromEnemy = true; // Sử dụng config từ enemy thay vì danh sách
    [SerializeField]
    private List<EnemyDamageConfig> enemyDamageConfigs = new List<EnemyDamageConfig>();

    void Awake()
    {
        // Tìm components cần thiết
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        FindHealthBarScript();
        UpdateHealthUI();
    }

    void FindHealthBarScript()
    {
        // Cách 1: Sử dụng tham chiếu trực tiếp nếu đã gán trong Inspector
        if (healthBarObject != null)
        {
            healthBarScript = healthBarObject.GetComponent<HealthBar>();
            if (healthBarScript != null)
            {
                Debug.Log("Đã tìm thấy HealthBar script từ tham chiếu được gán.");
                return;
            }
        }

        // Cách 2: Tìm trong GameManager
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            healthBarScript = gameManager.GetComponent<HealthBar>();
            if (healthBarScript != null)
            {
                Debug.Log("Tìm thấy HealthBar từ GameManager.");
                return;
            }
        }

        // Cách 3: Tìm theo tên GameObject
        GameObject healthBarGO = GameObject.Find("HealthBar");
        if (healthBarGO != null)
        {
            healthBarScript = healthBarGO.GetComponent<HealthBar>();
            if (healthBarScript != null)
            {
                Debug.Log("Tìm thấy HealthBar từ GameObject 'HealthBar'.");
                return;
            }
        }

        // Cách 4: Tìm theo tag
        GameObject taggedHealthBar = GameObject.FindWithTag("HealthBar");
        if (taggedHealthBar != null)
        {
            healthBarScript = taggedHealthBar.GetComponent<HealthBar>();
            if (healthBarScript != null)
            {
                Debug.Log("Tìm thấy HealthBar từ GameObject với tag 'HealthBar'.");
                return;
            }
        }

        // Cách 5: Tìm bất kỳ HealthBar nào trong scene
        healthBarScript = FindObjectOfType<HealthBar>();
        if (healthBarScript != null)
        {
            Debug.Log("Tìm thấy HealthBar từ bất kỳ GameObject nào trong scene.");
            return;
        }

        // Không tìm thấy
        Debug.LogWarning("Không tìm thấy HealthBar script trong scene. Vui lòng tạo HealthBar script hoặc gán nó trong Inspector.");
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead)
        {
            Debug.Log("Player đang trong trạng thái bất tử hoặc đã chết, không nhận sát thương");
            return;
        }

        Debug.Log("Player nhận " + damage + " sát thương!");

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        int healthLost = previousHealth - currentHealth;

        Debug.Log("Máu giảm từ " + previousHealth + " xuống " + currentHealth + " (mất " + healthLost + " máu)");

        UpdateHealthUI();


        if (animator != null)
        {
            Debug.Log("Kích hoạt animation hurt");
            animator.SetTrigger("hurt");
            AudioManager.Instance?.PlayPlayerHurt();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    void UpdateHealthUI()
    {
        Debug.Log("Đang cập nhật UI thanh máu...");

        if (healthBarScript != null)
        {
            healthBarScript.UpdateBar(currentHealth, maxHealth);
            Debug.Log("Đã cập nhật thanh máu thông qua HealthBar script");
        }
        else
        {
            // Nếu không tìm thấy HealthBar script, thử tạo và cập nhật UI theo cách khác
            UpdateHealthUIAlternative();
        }
    }

    // Phương thức dự phòng nếu không tìm thấy HealthBar script
    void UpdateHealthUIAlternative()
    {
        // Tìm Slider nếu có
        Slider healthSlider = GameObject.Find("HealthBar")?.GetComponent<Slider>();
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
            Debug.Log("Đã cập nhật Slider thanh máu");
            return;
        }

        // Hoặc tìm TextMeshProUGUI nếu có
        TextMeshProUGUI healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
            Debug.Log("Đã cập nhật Text thanh máu");
            return;
        }

        Debug.LogWarning("Không tìm thấy UI thanh máu nào để cập nhật.");
    }

    IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        if (spriteRenderer != null)
        {
            float elapsedTime = 0;
            while (elapsedTime < invincibleTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.1f;
            }
            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invincibleTime);
        }
        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        animator?.SetTrigger("die");

        AudioManager.Instance?.PlayPlayerDeath();
        ElaraAnimationController playerController = GetComponent<ElaraAnimationController>();
        playerController?.Die();

        if (healthBarScript != null)
        {
            healthBarScript.gameObject.SetActive(false);
        }

        Debug.Log("Player died");
    }

    public void ResetAfterRespawn()
    {
        isDead = false;
        isInvincible = false;
        currentHealth = maxHealth;

        if (healthBarScript != null)
        {
            healthBarScript.gameObject.SetActive(true);
            UpdateHealthUI();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
        Debug.Log($"Player được hồi {amount} máu. Máu hiện tại: {currentHealth}/{maxHealth}");

        AudioManager.Instance?.PlayHealthPotion();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleEnemyCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin"))
        {
            // Cách sửa 1: Sử dụng FindObjectOfType để tìm trực tiếp CoinManager
            CoinManager coinManager = FindObjectOfType<CoinManager>();
            if (coinManager != null)
            {
                coinManager.coinCount++;
                Debug.Log("PlayerHealth phát hiện coin và cập nhật coinCount");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy CoinManager trong scene");
            }
            // Thêm âm thanh nhặt coin
            AudioManager.Instance?.PlayCoinPickup();
            return;
        }

        HandleEnemyCollision(other.gameObject);
    }

    private int GetEnemyDamage(GameObject enemyObject)
    {
        if (useConfigFromEnemy)
        {
            EnemyAnimationController enemyController = enemyObject.GetComponent<EnemyAnimationController>();
            if (enemyController != null)
            {
                EnemyConfig config = enemyController.GetConfig();
                if (config != null)
                {
                    return Random.Range(config.minDamage, config.maxDamage + 1);
                }
            }
        }

        foreach (var config in enemyDamageConfigs)
        {
            if (config.enemyName == enemyObject.name)
            {
                return config.damage;
            }
        }

        return defaultEnemyDamage;
    }

    private void HandleEnemyCollision(GameObject otherObject)
    {
        if (otherObject.CompareTag("Enemy") || ((1 << otherObject.layer) & enemyLayer) != 0)
        {
            int damageAmount = GetEnemyDamage(otherObject);

            Debug.Log("Enemy " + otherObject.name + " gây " + damageAmount + " sát thương!");

            TakeDamage(damageAmount);
        }
    }
}
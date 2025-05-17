using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyConfig
{
    public string configName = "Default";
    public int health = 100;
    public int minDamage = 10;
    public int maxDamage = 20;
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    public float detectionRange = 3f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float respawnTime = 5f;
    public bool attackOnlyOnce = false;
}

[System.Serializable]
public class ItemDrop
{
    public GameObject itemPrefab;
    [Range(0f, 100f)]
    public float dropChance = 20f;
}

[ExecuteInEditMode]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [SerializeField] private EnemyConfig enemyConfig = new EnemyConfig();

    [Header("Current Stats")]
    [SerializeField] private int currentHealth;
    public float HealthPercentage => (float)currentHealth / enemyConfig.health * 100;

    [Header("Health Bar")]
    public GameObject healthBarContainer;
    public GameObject healthBarBackground;
    public GameObject healthBarFill;
    private float originalHealthBarWidth = 0.8f;

    [Header("Tham chiếu")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    public Transform attackPoint;

    [Header("Trạng thái")]
    private bool isHurt = false;
    private float hurtRecoveryTime = 0.7f;

    // Biến theo dõi và debug
    private float slashAnimationTime = 0.7f;
    private float currentSlashTime = 0f;
    private Vector3 deathPosition;

    // Biến lưu trữ
    private Vector3 startPosition;
    private bool movingRight = true;
    private float leftBoundary;
    private float rightBoundary;
    private bool isSlashing = false;
    private bool isDead = false;
    private GameObject playerObject;
    private bool playerInRange = false;
    private float attackTimer = 0f;
    private bool hasAttacked = false;

    // Public property để các script khác kiểm tra trạng thái
    public bool IsDead => isDead;

    // Layer của player để kiểm tra va chạm
    public LayerMask playerLayer;

    // Kiểm tra các animation parameter
    private bool hasWalkingParam = false;
    private bool hasSlashParam = false;
    private bool hasHurtParam = false;
    private bool hasDieParam = false;

    [Header("Item Drop Settings")]
    [SerializeField]
    private List<ItemDrop> itemDrops = new List<ItemDrop>
    {
        new ItemDrop { dropChance = 50f }, // Coin
        new ItemDrop { dropChance = 30f }, // Heart
        new ItemDrop { dropChance = 20f }, // Star
        new ItemDrop { dropChance = 10f }, // Key
        new ItemDrop { dropChance = 5f }   // Key_VIP
    };
    [SerializeField] private bool guaranteeOneDrop = true;

    void Awake()
    {
        if (!Application.isPlaying)
        {
            // Tạo hoặc tìm health bar trong Editor
            SetupHealthBarInEditor();
        }
    }

    void SetupHealthBarInEditor()
    {
        // Tìm health bar container nếu chưa có
        if (healthBarContainer == null)
        {
            Transform container = transform.Find("HealthBarContainer");
            if (container != null)
            {
                healthBarContainer = container.gameObject;
            }
            else
            {
                // Tạo mới health bar container
                healthBarContainer = new GameObject("HealthBarContainer");
                healthBarContainer.transform.SetParent(transform);
                healthBarContainer.transform.localPosition = new Vector3(0, 0.8f, 0);
            }
        }

        // Tìm hoặc tạo background
        if (healthBarBackground == null)
        {
            Transform bg = healthBarContainer.transform.Find("HealthBarBackground");
            if (bg != null)
            {
                healthBarBackground = bg.gameObject;
            }
            else
            {
                healthBarBackground = new GameObject("HealthBarBackground");
                healthBarBackground.transform.SetParent(healthBarContainer.transform);
                healthBarBackground.transform.localPosition = Vector3.zero;

                SpriteRenderer bgSprite = healthBarBackground.AddComponent<SpriteRenderer>();
                bgSprite.color = Color.black;
                bgSprite.sortingLayerName = "UI";
                bgSprite.sortingOrder = 1000;

                healthBarBackground.transform.localScale = new Vector3(0.8f, 0.08f, 1f);
            }
        }

        // Tìm hoặc tạo fill
        if (healthBarFill == null)
        {
            Transform fill = healthBarContainer.transform.Find("HealthBarFill");
            if (fill != null)
            {
                healthBarFill = fill.gameObject;
            }
            else
            {
                healthBarFill = new GameObject("HealthBarFill");
                healthBarFill.transform.SetParent(healthBarContainer.transform);
                healthBarFill.transform.localPosition = Vector3.zero;

                SpriteRenderer fillSprite = healthBarFill.AddComponent<SpriteRenderer>();
                fillSprite.color = Color.green;
                fillSprite.sortingLayerName = "UI";
                fillSprite.sortingOrder = 1001;

                healthBarFill.transform.localScale = new Vector3(0.8f, 0.08f, 1f);
            }
        }

        // Gán sprite mặc định nếu chưa có
        SetupDefaultSprites();
    }

    void SetupDefaultSprites()
    {
        // Kiểm tra và gán sprite cho background
        SpriteRenderer bgSprite = healthBarBackground.GetComponent<SpriteRenderer>();
        if (bgSprite != null && bgSprite.sprite == null)
        {
            // Tạo sprite trắng đơn giản
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            bgSprite.sprite = sprite;
        }

        // Kiểm tra và gán sprite cho fill
        SpriteRenderer fillSprite = healthBarFill.GetComponent<SpriteRenderer>();
        if (fillSprite != null && fillSprite.sprite == null)
        {
            if (bgSprite != null && bgSprite.sprite != null)
            {
                fillSprite.sprite = bgSprite.sprite;
            }
        }
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            Initialize();
            Debug.Log($"Left Boundary: {leftBoundary}, Right Boundary: {rightBoundary}");
            Debug.Log($"Current Position: {transform.position.x}");
        }
    }

    void Initialize()
    {
        // Lấy các component
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Thiết lập Rigidbody2D
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Kiểm tra các animation parameter
        CheckAnimationParameters();

        // Lưu vị trí bắt đầu
        startPosition = transform.position;

        // Tính toán phạm vi di chuyển
        leftBoundary = startPosition.x - enemyConfig.patrolDistance / 2;
        rightBoundary = startPosition.x + enemyConfig.patrolDistance / 2;

        // Khởi tạo trạng thái
        currentHealth = enemyConfig.health;
        isDead = false;
        isHurt = false;
        isSlashing = false;
        playerInRange = false;
        hasAttacked = false;
        currentSlashTime = 0f;

        // Đặt animation ban đầu là walking
        SetWalkingState(true);

        // Reset collider nếu đã bị disable
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;

        // Tìm player
        playerObject = GameObject.FindGameObjectWithTag("Player");

        // Tạo attack point nếu chưa có
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.parent = transform;
            attackPointObj.transform.localPosition = new Vector3(0.5f, 0, 0);
            attackPoint = attackPointObj.transform;
        }

        // Hiển thị health bar
        if (healthBarContainer != null)
            healthBarContainer.SetActive(true);

        // Cập nhật health bar ban đầu
        UpdateHealthBar();
    }

    void OnValidate()
    {
        // Cập nhật health bar trong Editor khi thay đổi giá trị
        if (!Application.isPlaying)
        {
            if (currentHealth == 0 || currentHealth > enemyConfig.health)
            {
                currentHealth = enemyConfig.health;
            }
            UpdateHealthBar();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercent = (float)currentHealth / enemyConfig.health;

            Vector3 newScale = healthBarFill.transform.localScale;
            newScale.x = originalHealthBarWidth * healthPercent;
            healthBarFill.transform.localScale = newScale;

            Vector3 newPosition = healthBarFill.transform.localPosition;
            newPosition.x = -(originalHealthBarWidth - newScale.x) / 2;
            healthBarFill.transform.localPosition = newPosition;

            SpriteRenderer fillSprite = healthBarFill.GetComponent<SpriteRenderer>();
            if (fillSprite != null)
            {
                if (healthPercent > 0.6f)
                    fillSprite.color = Color.green;
                else if (healthPercent > 0.3f)
                    fillSprite.color = Color.yellow;
                else
                    fillSprite.color = Color.red;
            }
        }
    }

    public void SetConfig(EnemyConfig newConfig)
    {
        enemyConfig = newConfig;
        Initialize();
    }

    public EnemyConfig GetConfig()
    {
        return enemyConfig;
    }

    void CheckAnimationParameters()
    {
        if (animator != null)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            foreach (AnimatorControllerParameter param in parameters)
            {
                if (param.name == "isWalking" && param.type == AnimatorControllerParameterType.Bool)
                    hasWalkingParam = true;
                else if (param.name == "slash" && param.type == AnimatorControllerParameterType.Trigger)
                    hasSlashParam = true;
                else if (param.name == "hurt" && param.type == AnimatorControllerParameterType.Trigger)
                    hasHurtParam = true;
                else if (param.name == "die" && param.type == AnimatorControllerParameterType.Trigger)
                    hasDieParam = true;
            }
        }
    }

    void FixedUpdate()
    {
        // QUAN TRỌNG: Chỉ chạy khi game đang play
        if (!Application.isPlaying)
            return;

        if (!isDead && rb != null)
        {
            if (!isSlashing && !isHurt && playerInRange)
            {
                rb.linearVelocity = Vector2.zero;
            }
            else if (isSlashing || isHurt)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void Update()
    {
        // QUAN TRỌNG: Chỉ chạy logic game khi đang play
        if (!Application.isPlaying)
            return;

        if (isDead)
            return;

        if (isHurt)
            return;

        if (isSlashing)
        {
            currentSlashTime += Time.deltaTime;
            if (currentSlashTime >= slashAnimationTime * 1.5f)
            {
                isSlashing = false;
                currentSlashTime = 0f;

                if (animator != null && hasSlashParam)
                    animator.ResetTrigger("slash");

                SetWalkingState(true);
            }
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        bool wasInRange = playerInRange;
        CheckPlayerRange();

        bool shouldAttack = playerInRange && !isSlashing && attackTimer <= 0;

        if (shouldAttack && playerObject == null)
        {
            shouldAttack = false;
        }

        if (shouldAttack && playerObject != null)
        {
            float actualDistance = Vector2.Distance(transform.position, playerObject.transform.position);
            if (actualDistance > enemyConfig.detectionRange)
            {
                shouldAttack = false;
            }
        }

        if (enemyConfig.attackOnlyOnce && hasAttacked)
        {
            shouldAttack = false;
        }

        if (shouldAttack)
        {
            SetWalkingState(false);

            if (playerObject != null)
            {
                spriteRenderer.flipX = (playerObject.transform.position.x < transform.position.x);

                if (attackPoint != null)
                {
                    Vector3 localPos = attackPoint.localPosition;
                    localPos.x = spriteRenderer.flipX ? -Mathf.Abs(localPos.x) : Mathf.Abs(localPos.x);
                    attackPoint.localPosition = localPos;
                }
            }

            Attack();

            if (enemyConfig.attackOnlyOnce)
                hasAttacked = true;
        }
        else if (!isSlashing)
        {
            SetWalkingState(true);
            Patrol();
        }
    }

    void SetWalkingState(bool isWalking)
    {
        if (hasWalkingParam && animator != null)
            animator.SetBool("isWalking", isWalking);
    }

    void CheckPlayerRange()
    {
        playerInRange = false;

        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null) return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerObject.transform.position);

        if (distanceToPlayer <= enemyConfig.detectionRange)
        {
            playerInRange = true;
        }
    }

    void Patrol()
    {
        // Kiểm tra giới hạn trước khi di chuyển
        float nextPosition = transform.position.x;

        if (movingRight)
        {
            nextPosition += enemyConfig.moveSpeed * Time.deltaTime;

            // Kiểm tra xem có vượt quá giới hạn không
            if (nextPosition > rightBoundary)
            {
                movingRight = false;
                return; // Không di chuyển nữa
            }

            transform.position = new Vector3(nextPosition, transform.position.y, transform.position.z);
            spriteRenderer.flipX = false;
        }
        else
        {
            nextPosition -= enemyConfig.moveSpeed * Time.deltaTime;

            // Kiểm tra xem có vượt quá giới hạn không
            if (nextPosition < leftBoundary)
            {
                movingRight = true;
                return; // Không di chuyển nữa
            }

            transform.position = new Vector3(nextPosition, transform.position.y, transform.position.z);
            spriteRenderer.flipX = true;
        }
    }

    void Attack()
    {
        if (!playerInRange || playerObject == null)
        {
            return;
        }

        currentSlashTime = 0f;

        if (hasSlashParam && animator != null)
        {
            animator.ResetTrigger("slash");
            animator.SetTrigger("slash");
        }
        AudioManager.Instance?.PlayEnemySlash();

        isSlashing = true;
        attackTimer = enemyConfig.attackCooldown;

        StartCoroutine(AutoEndSlashAnimation());
    }

    public void DealDamageToPlayer()
    {
        Vector2 attackPosition = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPosition, enemyConfig.attackRange, playerLayer);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            int damage = Random.Range(enemyConfig.minDamage, enemyConfig.maxDamage + 1);

            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                playerHealth = playerCollider.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    public void OnSlashAnimationMiddle()
    {
        if (isSlashing)
        {
            DealDamageToPlayer();
        }
    }

    private IEnumerator AutoEndSlashAnimation()
    {
        yield return new WaitForSeconds(slashAnimationTime);

        if (isSlashing)
        {
            OnSlashAnimationEnd();
        }
    }

    public void OnSlashAnimationEnd()
    {
        isSlashing = false;
        currentSlashTime = 0f;

        if (playerInRange && !enemyConfig.attackOnlyOnce)
        {
            SetWalkingState(false);
        }
        else
        {
            SetWalkingState(true);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        UpdateHealthBar();

        Debug.Log(gameObject.name + " nhận " + damage + " sát thương, máu còn lại: " + currentHealth);

        if (hasHurtParam && !isHurt)
        {
            animator.SetTrigger("hurt");
            isHurt = true;
            isSlashing = false;
            currentSlashTime = 0f;
            SetWalkingState(false);
            AudioManager.Instance?.PlayEnemyHurt();
            StartCoroutine(ResetHurtState());
        }

        if (currentHealth <= 0)
        {
            // Lưu vị trí hiện tại trước khi chạy animation chết
            Vector3 exactPosition = transform.position;
            Die(exactPosition);
        }
    }

    private IEnumerator ResetHurtState()
    {
        yield return new WaitForSeconds(hurtRecoveryTime);

        isHurt = false;

        if (enemyConfig.attackOnlyOnce)
            hasAttacked = false;

        if (playerInRange && !enemyConfig.attackOnlyOnce)
        {
            SetWalkingState(false);
        }
        else
        {
            SetWalkingState(true);
        }
    }

    public void Die(Vector3 exactPosition)
    {
        // Code hiện tại
        isDead = true;
        deathPosition = transform.position; // Lưu vị trí này để respawn sau này

        AudioManager.Instance?.PlayEnemyDeath();

        isSlashing = false;
        isHurt = false;
        currentSlashTime = 0f;

        SetWalkingState(false);

        if (hasDieParam)
            animator.SetTrigger("die");

        // Ẩn health bar khi chết
        if (healthBarContainer != null)
            healthBarContainer.SetActive(false);

        GetComponent<Collider2D>().enabled = false;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            rb.AddForce(new Vector2(0, -1f), ForceMode2D.Impulse);
        }

        Debug.Log(gameObject.name + " đã chết tại vị trí " + exactPosition);

        // Đây là phần quan trọng: rơi vật phẩm tại vị trí chính xác khi enemy còn sống
        DropItems(exactPosition);

        // THÊM VÀO ĐÂY: Gọi phương thức RegisterDefeat từ BossDefeatTracker
        BossDefeatTracker bossTracker = GetComponent<BossDefeatTracker>();
        if (bossTracker != null)
        {
            bossTracker.RegisterDefeat();
            Debug.Log("Đã gọi RegisterDefeat() để ghi nhận đánh bại boss");
        }

        StartCoroutine(RespawnAfterDelay());
    }

    // Hàm Die trống không có tham số cho tương thích với các gọi hàm khác (nếu có)
    public void Die()
    {
        Vector3 exactPosition = transform.position;

        // THÊM VÀO ĐÂY: Gọi RegisterDefeat trước khi chuyển tới Die(exactPosition)
        BossDefeatTracker bossTracker = GetComponent<BossDefeatTracker>();
        if (bossTracker != null)
        {
            bossTracker.RegisterDefeat();
            Debug.Log("Đã gọi RegisterDefeat() từ Die() không tham số");
        }

        Die(exactPosition);
    }


    private void DropItems(Vector3 dropPosition)
    {
        // Xóa tất cả các vật phẩm đã tồn tại trên bản đồ (tùy chọn, hãy bỏ comment nếu muốn sử dụng)
        // ClearExistingItems();

        // Chỉ rơi ra MỘT item duy nhất, có độ ưu tiên
        GameObject itemToDrop = null;

        // Xác định xem đã có một item được chọn chưa
        bool itemSelected = false;

        // Tạo danh sách ưu tiên: Key > Key_VIP > Star > Heart > Coin
        GameObject[] priorityItems = new GameObject[itemDrops.Count];
        float[] priorityChances = new float[itemDrops.Count];

        // Tạo danh sách priority dựa trên tên của prefab
        for (int i = 0; i < itemDrops.Count; i++)
        {
            if (itemDrops[i].itemPrefab != null)
            {
                priorityItems[i] = itemDrops[i].itemPrefab;

                // Giảm tỷ lệ rơi đồ xuống rất thấp
                priorityChances[i] = itemDrops[i].dropChance * 0.2f;

                // Ưu tiên Key và Key_VIP
                string itemName = itemDrops[i].itemPrefab.name.ToLower();
                if (itemName.Contains("key_vip"))
                {
                    priorityChances[i] = 5f; // 5% cơ hội
                }
                else if (itemName.Contains("key"))
                {
                    priorityChances[i] = 10f; // 10% cơ hội
                }
                else if (itemName.Contains("star"))
                {
                    priorityChances[i] = 15f; // 15% cơ hội
                }
                else if (itemName.Contains("heart"))
                {
                    priorityChances[i] = 20f; // 20% cơ hội
                }
                else if (itemName.Contains("coin"))
                {
                    priorityChances[i] = 30f; // 30% cơ hội
                }
            }
        }

        // Kiểm tra theo thứ tự ưu tiên
        for (int i = 0; i < priorityItems.Length; i++)
        {
            if (priorityItems[i] != null && !itemSelected)
            {
                if (Random.Range(0f, 100f) <= priorityChances[i])
                {
                    itemToDrop = priorityItems[i];
                    itemSelected = true;
                    break;
                }
            }
        }

        // Nếu không có item nào được chọn và cài đặt đảm bảo có ít nhất 1 item
        if (itemToDrop == null && guaranteeOneDrop)
        {
            // Ưu tiên rơi coin nếu có
            for (int i = 0; i < priorityItems.Length; i++)
            {
                if (priorityItems[i] != null)
                {
                    string itemName = priorityItems[i].name.ToLower();
                    if (itemName.Contains("coin"))
                    {
                        itemToDrop = priorityItems[i];
                        break;
                    }
                }
            }

            // Nếu không tìm thấy coin, lấy item đầu tiên có sẵn
            if (itemToDrop == null)
            {
                for (int i = 0; i < priorityItems.Length; i++)
                {
                    if (priorityItems[i] != null)
                    {
                        itemToDrop = priorityItems[i];
                        break;
                    }
                }
            }
        }

        // Nếu có item để rơi, sinh ra nó
        if (itemToDrop != null)
        {
            // Vị trí rơi chính xác tại vị trí enemy
            Vector3 exactSpawnPosition = new Vector3(
                dropPosition.x,
                dropPosition.y + 0.2f, // Nhấc lên một chút so với mặt đất
                dropPosition.z
            );

            SpawnItem(itemToDrop, exactSpawnPosition);
        }
    }

    // Hàm để xóa tất cả các vật phẩm hiện có (tùy chọn sử dụng)
    private void ClearExistingItems()
    {
        // Tìm tất cả các gameobject có tag liên quan
        string[] itemTags = new string[] { "Coin", "Heart", "Star", "Key", "Key_vip" };

        foreach (string tag in itemTags)
        {
            GameObject[] existingItems = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject item in existingItems)
            {
                // Kiểm tra khoảng cách từ vị trí enemy đến item
                // Chỉ xóa những item gần enemy (trong phạm vi 10 đơn vị)
                if (Vector3.Distance(transform.position, item.transform.position) < 10f)
                {
                    Destroy(item);
                }
            }
        }
    }

    private void SpawnItem(GameObject itemPrefab, Vector3 position)
    {
        // Sinh ra item tại vị trí được chỉ định
        GameObject item = Instantiate(itemPrefab, position, Quaternion.identity);

        // Thêm một chút hiệu ứng "bật lên" khi item rơi ra
        Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
        if (itemRb != null)
        {
            // Đảm bảo gravity được bật để item rơi xuống đất
            itemRb.isKinematic = false;
            itemRb.gravityScale = 1f;

            // Thêm lực bật lên rất nhẹ và hầu như không có lực ngang
            float randomX = Random.Range(-0.1f, 0.1f); // Gần như không có lực ngang
            float randomY = Random.Range(0.5f, 1f); // Lực bật lên rất nhỏ

            itemRb.AddForce(new Vector2(randomX, randomY), ForceMode2D.Impulse);
        }

        Debug.Log($"Đã spawn item {itemPrefab.name} tại vị trí chính xác: {position}");

        // Đảm bảo item sẽ bị hủy sau một khoảng thời gian nếu không được thu thập
        Destroy(item, 10f);
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(enemyConfig.respawnTime);

        transform.position = deathPosition;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        Initialize();

        // Hiện lại health bar sau khi respawn
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(true);
            UpdateHealthBar();
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            if (hasSlashParam)
                animator.ResetTrigger("slash");
            if (hasHurtParam)
                animator.ResetTrigger("hurt");
            if (hasDieParam)
                animator.ResetTrigger("die");

            SetWalkingState(true);
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;

        Debug.Log(gameObject.name + " đã hồi sinh tại vị trí " + transform.position);
    }

    public void OnAttackHitFrame()
    {
        DealDamageToPlayer();
    }

    public void ResetEnemyState()
    {
        StopAllCoroutines();

        isSlashing = false;
        isHurt = false;
        currentSlashTime = 0f;
        attackTimer = 0f;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            if (hasSlashParam)
                animator.ResetTrigger("slash");
            if (hasHurtParam)
                animator.ResetTrigger("hurt");
            if (hasDieParam)
                animator.ResetTrigger("die");
        }

        SetWalkingState(true);
    }

    public void DebugState()
    {
        Debug.Log(gameObject.name + " - Config: " + enemyConfig.configName);
        Debug.Log("Health: " + currentHealth + "/" + enemyConfig.health + " (" + HealthPercentage.ToString("F1") + "%)");
        Debug.Log("Damage: " + enemyConfig.minDamage + "-" + enemyConfig.maxDamage);
        Debug.Log("isDead: " + isDead);
        Debug.Log("isHurt: " + isHurt);
        Debug.Log("isSlashing: " + isSlashing);
        Debug.Log("playerInRange: " + playerInRange);

        if (playerObject != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerObject.transform.position);
            Debug.Log("Khoảng cách đến player: " + distanceToPlayer);
            Debug.Log("detectionRange: " + enemyConfig.detectionRange);
        }
    }

    void OnDrawGizmos()
    {
        if (enemyConfig == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyConfig.detectionRange);

        Gizmos.color = Color.red;
        Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(attackPos, enemyConfig.attackRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(new Vector3(leftBoundary, transform.position.y, 0),
                           new Vector3(rightBoundary, transform.position.y, 0));
        }
    }

    void OnDrawGizmosSelected()
    {
        if (enemyConfig == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyConfig.detectionRange);

        Gizmos.color = Color.red;
        Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(attackPos, enemyConfig.attackRange);

        Gizmos.color = Color.blue;
        Vector3 startPos = Application.isPlaying ? new Vector3(leftBoundary, transform.position.y, 0)
                                                : new Vector3(transform.position.x - enemyConfig.patrolDistance / 2, transform.position.y, 0);
        Vector3 endPos = Application.isPlaying ? new Vector3(rightBoundary, transform.position.y, 0)
                                              : new Vector3(transform.position.x + enemyConfig.patrolDistance / 2, transform.position.y, 0);
        Gizmos.DrawLine(startPos, endPos);

        if (playerObject != null && Application.isPlaying)
        {
            Gizmos.color = playerInRange ? Color.green : Color.gray;
            Gizmos.DrawLine(transform.position, playerObject.transform.position);
        }
    }
}
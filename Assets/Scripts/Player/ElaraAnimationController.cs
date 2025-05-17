using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ElaraAnimationController : MonoBehaviour
{
    // Các component cần thiết
    [Header("Components")]
    private Animator animator;      // Điều khiển animation của nhân vật
    private Rigidbody2D rb;         // Xử lý vật lý 2D cho nhân vật

    // Các thông số di chuyển, có thể điều chỉnh trong Inspector
    [Header("Movement Settings")]
    public float walkSpeed = 3f;    // Tốc độ đi bộ
    public float runSpeed = 6f;     // Tốc độ chạy
    public float jumpForce = 10f;   // Lực nhảy

    // Thiết lập kiểm tra mặt đất
    [Header("Ground Check")]
    public Transform groundCheck;    // Vị trí kiểm tra tiếp xúc với mặt đất
    public float groundCheckRadius = 0.2f;  // Bán kính kiểm tra
    public LayerMask groundLayer;    // Layer được coi là mặt đất

    // Các biến trạng thái nội bộ
    private bool isGrounded;         // Nhân vật có đang đứng trên mặt đất?
    private bool canDoubleJump;      // Nhân vật có thể thực hiện double jump?
    private bool facingRight = true; // Mặc định nhân vật nhìn sang phải
    private float lastJumpTime = 0f;
    private bool hasJumpedOnce = false; // Đánh dấu đã nhảy một lần

    // TRANG THAI TAN CONG
    public Transform attackPoint; // Điểm tấn công
    public LayerMask enemyLayer; // Layer của kẻ thù

    // Thay đổi: Tăng tầm tấn công từ cận chiến thành đánh xa
    public float attackRange = 10f; // Bán kính tấn công (đã tăng lên)
    public int attackDamage = 100; // sat thuong

    [Header("Respawn Settings")]
    public Transform respawnPoint; // Điểm hồi sinh (có thể gán trong Inspector)
    public float respawnDelay = 2f; // Thời gian chờ trước khi hồi sinh
    private Vector3 startPosition; // Vị trí ban đầu của nhân vật
    private bool isDead = false; // Trạng thái chết của nhân vật

    // THÊM MỚI: Thiết lập cho việc bắn tên
    [Header("Arrow Settings")]
    public GameObject arrowPrefab; // Prefab của mũi tên
    public Transform shootPoint; // Điểm bắn mũi tên
    public float arrowSpeed = 10f; // Tốc độ bay của mũi tên

    // THÊM MỚI: Cooldown thời gian bắn
    [Header("Shoot Cooldown")]
    public float shootCooldown = 0.5f; // Thời gian giữa các lần bắn (0.5s)
    private float lastShootTime = 0f; // Thời điểm bắn cuối cùng
    public bool showShootCooldown = true; // Hiển thị debug cooldown

    [Header("Trap Settings")]
    public LayerMask trapLayer;    // Layer của bẫy
    public int trapDamage = 5;     // Sát thương mỗi lần
    public float trapDamageInterval = 3f; // Thời gian giữa các lần gây sát thương
    private bool isOnTrap = false; // Đang đứng trên bẫy không?
    private float trapDamageTimer = 0f; // Đếm thời gian để gây sát thương

    public PlayerHealth playerHealth;

    private float footstepTimer = 0f;
    private float footstepInterval = 0.3f; // Thời gian giữa các bước chân khi đi bộ
    private float runFootstepInterval = 0.2f; // Thời gian giữa các bước chân khi chạy

    // Biến debug cho double jump
    [Header("Debug")]
    public bool debugDoubleJump = true; // Bật/tắt debug cho double jump

    void Start()
    {
        // Lưu vị trí ban đầu của nhân vật
        startPosition = transform.position;

        if (respawnPoint == null)
        {
            Debug.Log("Respawn point not set, using start position");
        }

        // Lấy các component cần thiết
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Tạo groundCheck nếu chưa có
        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = check.transform;
            Debug.Log("Created GroundCheck object");
        }

        // THÊM MỚI: Tạo shootPoint nếu chưa có
        if (shootPoint == null)
        {
            GameObject shoot = new GameObject("ShootPoint");
            shoot.transform.parent = transform;
            shoot.transform.localPosition = new Vector3(0.5f, 0f, 0);
            shootPoint = shoot.transform;
            Debug.Log("Created ShootPoint object");
        }

        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = gameObject.AddComponent<PlayerHealth>();
        }

        // Reset trạng thái nhảy ban đầu
        hasJumpedOnce = false;
        canDoubleJump = false;

        // Khởi tạo thời gian bắn cuối cùng
        lastShootTime = -shootCooldown; // Cho phép bắn ngay lập tức khi bắt đầu game
    }

    public void Die()
    {
        if (isDead) return; // Tránh gọi die nhiều lần

        isDead = true;

        // Phát âm thanh chết
        AudioManager.Instance?.PlayPlayerDeath();

        // THAY ĐỔI QUAN TRỌNG: Không chạy animation die, chỉ đặt trạng thái
        // Vô hiệu hóa animator để ngăn animation die lặp lại
        if (animator != null)
        {
            animator.enabled = false; // Tắt animator để không chạy animation nữa
        }

        // Vô hiệu hóa script điều khiển để người chơi không điều khiển được nữa
        this.enabled = false;

        // Cho phép nhân vật rơi xuống với physics thực tế
        if (rb != null)
        {
            // Duy trì gravity bình thường để nhân vật rơi tự nhiên
            rb.gravityScale = 2f; // Tăng nhẹ để rơi nhanh hơn một chút

            // Cho phép nhân vật xoay khi rơi
            rb.constraints = RigidbodyConstraints2D.None; // Bỏ constraint để nhân vật có thể xoay

            // Thêm lực đẩy ngược lại và hơi lên trên
            float knockbackForce = facingRight ? -2f : 2f; // Lực đẩy ngược với hướng nhìn
            rb.AddForce(new Vector2(knockbackForce, 4f), ForceMode2D.Impulse);

            // Thêm lực xoay khi rơi
            rb.AddTorque(facingRight ? -10f : 10f, ForceMode2D.Impulse);
        }

        // Vô hiệu hóa collider ngay lập tức hoặc sau một thời gian ngắn
        StartCoroutine(DisableColliderAfterDelay(0.2f));

        // Lưu thông tin vị trí để PlayerSpawner có thể sử dụng
        SaveRespawnPosition();

        Debug.Log("Player died, respawning in " + respawnDelay + " seconds");

        // Bắt đầu coroutine hồi sinh
        StartCoroutine(RespawnPlayer());
    }


    // Coroutine để vô hiệu hóa collider sau một khoảng thời gian
    private IEnumerator DisableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Vô hiệu hóa collider để nhân vật rơi qua các platform
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Kiểm tra collider con nếu có
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in childColliders)
        {
            col.enabled = false;
        }
    }

    // Lưu vị trí hồi sinh vào PlayerPrefs
    private void SaveRespawnPosition()
    {
        // Đặt flag để PlayerSpawner biết cần sử dụng vị trí tùy chỉnh
        PlayerPrefs.SetInt("UseCustomSpawn", 1);

        // Mặc định đặt tại vị trí respawnPoint nếu có
        if (respawnPoint != null)
        {
            PlayerPrefs.SetFloat("SpawnPositionX", respawnPoint.position.x);
            PlayerPrefs.SetFloat("SpawnPositionY", respawnPoint.position.y);
            Debug.Log("Saved respawn position from respawnPoint: " + respawnPoint.position);
        }
        else
        {
            // Nếu không có respawnPoint, sử dụng vị trí khởi tạo ban đầu
            PlayerPrefs.SetFloat("SpawnPositionX", startPosition.x);
            PlayerPrefs.SetFloat("SpawnPositionY", startPosition.y);
            Debug.Log("Saved respawn position from startPosition: " + startPosition);
        }

        // Lưu các thông tin khác nếu cần
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (isDead) return; // Không xử lý gì khi đã chết

        // Kiểm tra trạng thái đứng trên mặt đất
        CheckGrounded();

        CheckTrap();

        // Debug thông tin double jump khi nhấn phím nhảy
        if (debugDoubleJump && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Jump key pressed - Grounded: " + isGrounded + ", CanDoubleJump: " + canDoubleJump + ", HasJumpedOnce: " + hasJumpedOnce);
        }

        HandleFootstepSounds();

        // Xử lý input của người chơi
        HandleInput();

        // Cập nhật animator với các trạng thái hiện tại
        UpdateAnimator();
    }

    // Coroutine để hồi sinh nhân vật
    private IEnumerator RespawnPlayer()
    {
        // Đợi một khoảng thời gian trước khi hồi sinh
        yield return new WaitForSeconds(respawnDelay);

        // Load scene home - Phải đặt tên scene đúng với tên trong Build Settings
        SceneManager.LoadScene("HomeScene"); // Thay "HomeScene" bằng tên scene home của bạn
    }


    void FixedUpdate()
    {
        // Xử lý di chuyển vật lý
        HandleMovement();
    }

    void CheckGrounded()
    {
        // Kiểm tra va chạm với mặt đất bằng OverlapCircle
        bool prevGrounded = isGrounded;

        // Đảm bảo groundCheck và groundLayer được thiết lập đúng
        if (groundCheck != null)
        {
            LayerMask combinedLayer = groundLayer | trapLayer;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, combinedLayer);

            // Debug khi trạng thái mặt đất thay đổi
            if (isGrounded && !prevGrounded && debugDoubleJump)
            {
                Debug.Log("Landed on ground - resetting jump states");
            }
        }
        else
        {
            Debug.LogError("GroundCheck is missing!");
            return;
        }

        // Khi nhân vật chạm đất sau khi nhảy hoặc rơi
        if (isGrounded && !prevGrounded)
        {
            // Reset các trạng thái nhảy và rơi
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);

            // Kích hoạt animation tiếp đất nếu cần
            if (rb.linearVelocity.y < 0)
            {
                animator.SetTrigger("land");

                // Reset trạng thái nhảy khi chạm đất sau khi rơi
                hasJumpedOnce = false;
                canDoubleJump = false;
            }
        }
    }

    void HandleInput()
    {
        if (isDead) return; // Không xử lý input khi đã chết

        // Xử lý nhảy khi nhấn nút Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float currentTime = Time.time;

            if (isGrounded)  // Nhảy thường khi đang đứng trên mặt đất
            {
                // Thực hiện nhảy lần đầu
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger("jumpStart");
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);

                // Cập nhật trạng thái nhảy
                lastJumpTime = currentTime;
                hasJumpedOnce = true;  // Đánh dấu đã nhảy một lần
                canDoubleJump = true;  // Cho phép double jump sau nhảy đầu tiên

                if (debugDoubleJump)
                {
                    Debug.Log("First jump performed - canDoubleJump set to true");
                }
                AudioManager.Instance?.PlayJump();
            }
            else if (canDoubleJump)  // Thực hiện double jump nếu được phép
            {
                // Thực hiện nhảy lần thứ hai
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.8f);
                animator.SetTrigger("doubleJump");

                // Cập nhật trạng thái double jump
                canDoubleJump = false; // Đã sử dụng double jump

                if (debugDoubleJump)
                {
                    Debug.Log("Double jump performed!");
                }
                AudioManager.Instance?.PlayDoubleJump();
            }
            else if (debugDoubleJump)
            {
                Debug.Log("Cannot double jump - canDoubleJump is false");
            }
        }

        // Xử lý bắn tên - ĐÃ THÊM COOLDOWN
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Kiểm tra xem đã qua thời gian cooldown chưa
            float currentTime = Time.time;
            float timeSinceLastShoot = currentTime - lastShootTime;

            if (timeSinceLastShoot >= shootCooldown)
            {
                // Đã qua thời gian cooldown, có thể bắn
                animator.SetTrigger("shoot");
                ShootArrow();
                lastShootTime = currentTime; // Cập nhật thời gian bắn cuối cùng

                if (showShootCooldown)
                {
                    Debug.Log("Arrow fired. Next arrow available in " + shootCooldown + " seconds");
                }
            }
            else if (showShootCooldown)
            {
                // Chưa hết cooldown, hiển thị thông báo debug
                float remainingCooldown = shootCooldown - timeSinceLastShoot;
                Debug.Log("Cannot shoot yet. Cooldown remaining: " + remainingCooldown.ToString("F2") + " seconds");
            }
        }
    }

    void ShootArrow()
    {
        if (arrowPrefab != null && shootPoint != null)
        {
            // Log giá trị sát thương trước khi bắn
            Debug.Log("Player attack damage before shooting: " + attackDamage);

            // Tạo mũi tên tại vị trí shootPoint
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, shootPoint.rotation);

            // Điều chỉnh scale mũi tên theo hướng nhân vật
            Vector3 arrowScale = arrow.transform.localScale;
            if (!facingRight)
            {
                arrowScale.x *= -1;
            }
            arrow.transform.localScale = arrowScale;

            // Lấy hoặc thêm component Arrow
            Arrow arrowComponent = arrow.GetComponent<Arrow>();
            if (arrowComponent != null)
            {
                // Đặt sát thương cho mũi tên từ attackDamage của nhân vật
                arrowComponent.SetDamage(attackDamage);
                Debug.Log("Set arrow damage to: " + attackDamage);
            }
            else
            {
                Debug.LogError("Arrow prefab does not have Arrow script attached!");
            }

            // Lấy hoặc thêm Rigidbody2D cho mũi tên
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb == null)
            {
                arrowRb = arrow.AddComponent<Rigidbody2D>();
                arrowRb.gravityScale = 0;
            }

            // Đặt vận tốc cho mũi tên dựa trên hướng nhân vật
            float direction = facingRight ? 1f : -1f;
            arrowRb.linearVelocity = new Vector2(direction * arrowSpeed, 0);

            // Hủy mũi tên sau 5 giây nếu không va chạm với gì
            Destroy(arrow, 5f);

            AudioManager.Instance?.PlayShoot();
        }
        else
        {
            Debug.LogError("Arrow prefab or shoot point is not set!");
        }
    }

    // THÊM MỚI: Phương thức để lấy thời gian cooldown còn lại
    public float GetRemainingShootCooldown()
    {
        float currentTime = Time.time;
        float timeSinceLastShoot = currentTime - lastShootTime;
        float remainingCooldown = Mathf.Max(0, shootCooldown - timeSinceLastShoot);

        return remainingCooldown;
    }

    // THÊM MỚI: Phương thức kiểm tra xem có thể bắn không
    public bool CanShoot()
    {
        return GetRemainingShootCooldown() <= 0;
    }

    void CheckTrap()
    {
        if (groundCheck != null)
        {
            isOnTrap = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, trapLayer);

            if (isOnTrap)
            {
                trapDamageTimer += Time.deltaTime;

                if (trapDamageTimer >= trapDamageInterval)
                {
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(trapDamage);
                        Debug.Log("Player bị trap gây " + trapDamage + " sát thương!");
                    }

                    trapDamageTimer = 0f;
                }
            }
            else
            {
                trapDamageTimer = 0f;
            }
        }
    }

    void HandleMovement()
    {
        if (isDead) return; // Không cho di chuyển khi đã chết

        float moveInput = Input.GetAxisRaw("Horizontal");

        bool canMove = !animator.GetCurrentAnimatorStateInfo(0).IsName("Die");

        if (!canMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(moveInput) > 0.1f;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);

        if (moveInput != 0)
        {
            bool shouldFaceRight = moveInput > 0;
            if (facingRight != shouldFaceRight)
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void UpdateAnimator()
    {
        float moveInput = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);

        bool isWalking = moveInput > 0.1f && !isShiftPressed;
        bool isRunning = moveInput > 0.1f && isShiftPressed;

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetFloat("horizontalSpeed", moveInput);

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", true);
            }
        }

        animator.SetFloat("verticalVelocity", rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Application.isPlaying && isGrounded ? Color.green : Color.black;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

            if (isOnTrap)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius * 1.1f);
            }
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        if (shootPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(shootPoint.position, 0.1f);
            Gizmos.DrawRay(shootPoint.position, transform.right * 3f);

            // THÊM MỚI: Vẽ thêm chỉ báo cooldown nếu đang trong game
            if (Application.isPlaying && showShootCooldown)
            {
                float cooldownRemaining = GetRemainingShootCooldown();
                if (cooldownRemaining > 0)
                {
                    // Vẽ chỉ báo cooldown (màu đỏ khi không thể bắn)
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(shootPoint.position, 0.15f);
                }
                else
                {
                    // Vẽ chỉ báo sẵn sàng bắn (màu xanh lá)
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(shootPoint.position, 0.15f);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Đã loại bỏ xử lý coin/chest ở đây để tránh tính điểm 2 lần
        // Việc thu thập coin/chest đã được xử lý bởi CollectItems.cs

        // Chỉ giữ lại các xử lý khác nếu cần trong tương lai
        // Ví dụ: xử lý va chạm với kẻ thù, cổng dịch chuyển, v.v.
    }

    // Thêm phương thức mới vào cuối script
    private void HandleFootstepSounds()
    {
        if (isDead || !isGrounded)
            return;

        float moveInput = Mathf.Abs(Input.GetAxisRaw("Horizontal"));
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);
        bool isMoving = moveInput > 0.1f;

        if (isMoving)
        {
            float interval = isShiftPressed ? runFootstepInterval : footstepInterval;

            footstepTimer += Time.deltaTime;
            if (footstepTimer >= interval)
            {
                footstepTimer = 0f;
                if (isShiftPressed)
                {
                    AudioManager.Instance?.PlayRun();
                }
                else
                {
                    AudioManager.Instance?.PlayFootstep();
                }
            }
        }
        else
        {
            // Reset timer khi không di chuyển
            footstepTimer = 0f;
            AudioManager.Instance?.StopRun(); // Dừng âm thanh chạy nếu đang phát
        }
    }
}
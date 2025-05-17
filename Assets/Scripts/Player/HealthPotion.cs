using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Cài đặt bình máu")]
    public int healAmount = 300; // Hồi 300 máu theo yêu cầu
    public bool destroyOnPickup = true;

    [Header("Hiệu ứng")]
    public float bobSpeed = 1.0f;
    public float bobHeight = 0.1f;
    public float rotateSpeed = 0f; // Đặt > 0 nếu muốn bình quay

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Hiệu ứng di chuyển lên xuống
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Hiệu ứng xoay (nếu cần)
        if (rotateSpeed > 0)
        {
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tìm PlayerHealth component của player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Hồi máu cho player
                playerHealth.Heal(healAmount);
                Debug.Log($"Người chơi nhận được {healAmount} máu từ bình máu!");

                // Destroy bình máu sau khi sử dụng (nếu cần)
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
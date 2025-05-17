using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage = 100; // Giá trị mặc định là 100
    public float maxDistance = 10f; // Khoảng cách tối đa mũi tên có thể bay
    private Vector3 startPosition; // Vị trí ban đầu của mũi tên

    void Start()
    {
        // Lưu vị trí ban đầu khi mũi tên được tạo ra
        startPosition = transform.position;
        Debug.Log("Arrow created with damage: " + damage);
    }

    void Update()
    {
        // Tính khoảng cách từ vị trí hiện tại đến vị trí ban đầu
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        // Nếu mũi tên đã bay quá khoảng cách tối đa, hủy nó
        if (distanceTraveled >= maxDistance)
        {
            Debug.Log("Arrow reached maximum distance: " + distanceTraveled + " - destroying");
            Destroy(gameObject);
        }
    }

    // Phương thức này sẽ được gọi từ ElaraAnimationController để đặt sát thương
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
        Debug.Log("Arrow damage set to: " + damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Arrow hit: " + collision.gameObject.name + " with tag: " + collision.gameObject.tag);
        // Kiểm tra va chạm bằng Tag
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("Arrow hit enemy with damage: " + damage);
            // Gây sát thương cho kẻ địch
            EnemyAnimationController enemy = collision.GetComponent<EnemyAnimationController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Applied " + damage + " damage to enemy");
            }
            // Hủy mũi tên sau khi va chạm
            Destroy(gameObject);
        }
    }
}
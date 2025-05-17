using UnityEngine;
public class PlayerSpawner : MonoBehaviour
{
    // Flag để kiểm tra nhân vật đã được đặt vị trí chưa
    private bool positionSet = false;

    void Start()
    {
        // Chờ một khoảng thời gian dài hơn
        Invoke("SetPlayerPosition", 0.2f);
    }

    void Update()
    {
        // Kiểm tra liên tục trong vài frame đầu tiên
        if (!positionSet && Time.frameCount < 10)
        {
            SetPlayerPosition();
        }
    }

    void SetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (PlayerPrefs.GetInt("UseCustomSpawn", 0) == 1)
            {
                float x = PlayerPrefs.GetFloat("SpawnPositionX", 0f);
                float y = PlayerPrefs.GetFloat("SpawnPositionY", 0f);
                float z = player.transform.position.z;
                // Thử phương pháp khác nhau để đặt vị trí
                player.transform.position = new Vector3(x, y, z);
                // Thêm dòng này
                Debug.Log("TRƯỚC KHI ĐẶT: " + player.transform.position);
                // Force đặt vị trí bằng cách khác
                Transform playerTransform = player.transform;
                playerTransform.position = new Vector3(x, y, z);
                // Thêm dòng này
                Debug.Log("SAU KHI ĐẶT: " + player.transform.position);
                // Reset velocity nếu có Rigidbody2D
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.Sleep(); // Đưa rb về trạng thái nghỉ
                }
                // Thêm đoạn này để vô hiệu hóa physics tạm thời
                if (rb != null)
                {
                    bool wasKinematic = rb.isKinematic;
                    rb.isKinematic = true;
                    rb.position = new Vector2(x, y);
                    rb.isKinematic = wasKinematic;
                }
                Debug.Log("ĐÃ ĐẶT NHÂN VẬT TẠI: " + player.transform.position);
                // Đánh dấu đã đặt vị trí
                positionSet = true;
                // Reset flag
                PlayerPrefs.SetInt("UseCustomSpawn", 0);
            }
            else
            {
                Debug.Log("Sử dụng vị trí mặc định cho nhân vật");
                positionSet = true;
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Player trong scene!");
        }
    }
}
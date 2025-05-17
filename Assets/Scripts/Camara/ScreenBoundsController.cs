using UnityEngine;

public class ScreenBoundsController : MonoBehaviour
{
    [Header("Screen Bounds")]
    [Tooltip("Giới hạn bên trái của màn hình game")]
    public float leftBound = -8f;

    [Tooltip("Giới hạn bên phải của màn hình game")]
    public float rightBound = 8f;

    [Tooltip("Giới hạn bên trên của màn hình game")]
    public float topBound = 4.5f;

    [Tooltip("Giới hạn bên dưới của màn hình game")]
    public float bottomBound = -4.5f;

    [Header("Options")]
    [Tooltip("Tự động tính toán giới hạn dựa trên kích thước camera")]
    public bool autoCalculateBounds = false;

    [Tooltip("Tính offset từ biên màn hình (thêm vào để tránh nhân vật chạm biên)")]
    public float boundsOffset = 0.5f;

    // Tham chiếu đến transform của nhân vật
    private Transform playerTransform;

    // Kích thước nhân vật (dựa vào collider)
    private float playerWidth = 0f;
    private float playerHeight = 0f;

    private void Start()
    {
        // Lấy transform của đối tượng này
        playerTransform = transform;

        // Tính kích thước nhân vật dựa vào collider
        CalculatePlayerSize();

        // Tính toán giới hạn màn hình nếu cần
        if (autoCalculateBounds)
        {
            CalculateScreenBounds();
        }

        Debug.Log($"Screen bounds set: Left={leftBound}, Right={rightBound}, Top={topBound}, Bottom={bottomBound}");
    }

    private void CalculatePlayerSize()
    {
        // Lấy collider của player
        Collider2D playerCollider = GetComponent<Collider2D>();

        if (playerCollider != null)
        {
            // Lấy kích thước collider
            if (playerCollider is BoxCollider2D)
            {
                BoxCollider2D boxCollider = playerCollider as BoxCollider2D;
                playerWidth = boxCollider.size.x * transform.localScale.x / 2f;
                playerHeight = boxCollider.size.y * transform.localScale.y / 2f;
            }
            else if (playerCollider is CircleCollider2D)
            {
                CircleCollider2D circleCollider = playerCollider as CircleCollider2D;
                playerWidth = circleCollider.radius * transform.localScale.x;
                playerHeight = circleCollider.radius * transform.localScale.y;
            }
            else if (playerCollider is CapsuleCollider2D)
            {
                CapsuleCollider2D capsuleCollider = playerCollider as CapsuleCollider2D;
                playerWidth = capsuleCollider.size.x * transform.localScale.x / 2f;
                playerHeight = capsuleCollider.size.y * transform.localScale.y / 2f;
            }

            Debug.Log($"Player size calculated: Width={playerWidth}, Height={playerHeight}");
        }
        else
        {
            // Nếu không tìm thấy collider, sử dụng giá trị mặc định
            playerWidth = 0.5f;
            playerHeight = 0.5f;
            Debug.LogWarning("No collider found on player, using default size values.");
        }
    }

    private void CalculateScreenBounds()
    {
        // Lấy camera chính
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Tính chiều cao và chiều rộng của màn hình trong đơn vị world units
            float screenHeight = 2f * mainCamera.orthographicSize;
            float screenWidth = screenHeight * mainCamera.aspect;

            // Tính các giới hạn dựa vào vị trí camera
            Vector3 cameraPosition = mainCamera.transform.position;

            leftBound = cameraPosition.x - (screenWidth / 2f) + playerWidth + boundsOffset;
            rightBound = cameraPosition.x + (screenWidth / 2f) - playerWidth - boundsOffset;
            bottomBound = cameraPosition.y - (screenHeight / 2f) + playerHeight + boundsOffset;
            topBound = cameraPosition.y + (screenHeight / 2f) - playerHeight - boundsOffset;

            Debug.Log("Screen bounds calculated automatically based on camera.");
        }
        else
        {
            Debug.LogError("Main Camera not found! Cannot calculate screen bounds automatically.");
        }
    }

    private void LateUpdate()
    {
        // Giới hạn vị trí của nhân vật trong phạm vi màn hình
        ClampPositionToScreenBounds();
    }

    private void ClampPositionToScreenBounds()
    {
        // Lấy vị trí hiện tại
        Vector3 currentPosition = playerTransform.position;

        // Giới hạn vị trí trong phạm vi đã định
        float clampedX = Mathf.Clamp(currentPosition.x, leftBound, rightBound);
        float clampedY = Mathf.Clamp(currentPosition.y, bottomBound, topBound);

        // Tạo vị trí mới đã giới hạn
        Vector3 clampedPosition = new Vector3(clampedX, clampedY, currentPosition.z);

        // Chỉ cập nhật nếu vị trí thực sự thay đổi
        if (currentPosition != clampedPosition)
        {
            playerTransform.position = clampedPosition;
        }
    }

    // Hàm để vẽ gizmo hiển thị giới hạn màn hình trong Editor
    private void OnDrawGizmosSelected()
    {
        // Vẽ hình chữ nhật thể hiện giới hạn màn hình
        Gizmos.color = Color.yellow;

        // Vẽ các đường biên giới hạn
        Vector3 topLeft = new Vector3(leftBound, topBound, 0);
        Vector3 topRight = new Vector3(rightBound, topBound, 0);
        Vector3 bottomLeft = new Vector3(leftBound, bottomBound, 0);
        Vector3 bottomRight = new Vector3(rightBound, bottomBound, 0);

        Gizmos.DrawLine(topLeft, topRight);    // Đường trên
        Gizmos.DrawLine(topRight, bottomRight); // Đường phải
        Gizmos.DrawLine(bottomRight, bottomLeft); // Đường dưới
        Gizmos.DrawLine(bottomLeft, topLeft);   // Đường trái
    }
}
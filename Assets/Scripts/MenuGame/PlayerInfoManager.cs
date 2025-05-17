using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInfoManager : MonoBehaviour
{
    [Header("Bảng Thông Tin")]
    public GameObject playerInfoPanel; // Bảng thông tin nhân vật

    [Header("Văn Bản Chỉ Số")]
    public TextMeshProUGUI healthText;        // Text hiển thị máu
    public TextMeshProUGUI attackText;        // Text hiển thị sát thương
    public TextMeshProUGUI defenseText;       // Text hiển thị phòng thủ
    public TextMeshProUGUI speedText;         // Text hiển thị tốc độ

    [Header("Nút Điều Khiển")]
    public Button playerInfoButton;   // Nút mở thông tin nhân vật

    private void Start()
    {
        // Ẩn bảng thông tin khi bắt đầu
        if (playerInfoPanel != null)
            playerInfoPanel.SetActive(false);

        // Thêm sự kiện nhấn cho nút thông tin
        if (playerInfoButton != null)
        {
            playerInfoButton.onClick.AddListener(ShowPlayerInfoPanel);
        }
    }

    // Hiển thị bảng thông tin
    public void ShowPlayerInfoPanel()
    {
        // Cập nhật thông tin trước khi hiển thị
        UpdatePlayerInfo();

        // Hiển thị panel
        if (playerInfoPanel != null)
        {
            playerInfoPanel.SetActive(true);
        }
    }

    // Cập nhật thông tin nhân vật
    public void UpdatePlayerInfo()
    {
        // Tìm các component cần thiết
        ElaraAnimationController playerController = FindObjectOfType<ElaraAnimationController>();
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();

        // Kiểm tra xem đã tìm thấy các component chưa
        if (playerController != null && playerHealth != null)
        {
            // Cập nhật thông tin máu
            healthText.text = $"{playerHealth.currentHealth}/{playerHealth.maxHealth}";

            // Cập nhật thông tin tấn công
            int attackDamage = playerController.attackDamage;
            attackText.text = $"{attackDamage}";

            // Cập nhật thông tin tốc độ di chuyển
            float runSpeed = playerController.runSpeed;
            speedText.text = $"{runSpeed}";

            // Cập nhật thông tin phòng thủ (giá trị mặc định)
            int defense = 10; // Giá trị mặc định, bạn có thể thay đổi
            defenseText.text = $"{defense}";
        }
        else
        {
            // Nếu không tìm thấy component, sử dụng giá trị mặc định
            Debug.LogWarning("Không tìm thấy thông tin nhân vật. Sử dụng dữ liệu mặc định.");

            // Giá trị mặc định
            healthText.text = "100/100";
            attackText.text = "40";
            speedText.text = "6";
            defenseText.text = "10";
        }
    }
}
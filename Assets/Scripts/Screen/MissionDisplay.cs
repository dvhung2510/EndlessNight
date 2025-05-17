using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MissionDisplay : MonoBehaviour
{
    [Header("Mission Panel")]
    public GameObject missionPanel;
    public TextMeshProUGUI missionTitle;
    public TextMeshProUGUI coinRequirementText;
    public TextMeshProUGUI chestRequirementText;
    public TextMeshProUGUI bossRequirementText; // Thêm text cho yêu cầu boss
    public Button closeButton;

    [Header("Mission Complete")]
    public GameObject completionNotification;
    public TextMeshProUGUI completionText;
    public float notificationDuration = 3f;

    [Header("Game Winner")]
    // Thêm tham chiếu tới thông báo chiến thắng game
    public GameObject winnerNotification;
    public TextMeshProUGUI winnerText;
    // Reference tới các nút trong panel chiến thắng
    public Button resetGameButton;
    public Button exitGameButton;

    [Header("Settings")]
    public KeyCode toggleMissionKey = KeyCode.Tab;
    public bool showOnStart = true;
    public float updateInterval = 0.5f; // Cập nhật mỗi 0.5 giây

    private GameProgress gameProgress;
    private int currentMapIndex;
    private float updateTimer = 0f;
    // Flag để tránh hiển thị thông báo nhiều lần
    private bool hasShownCompletionNotification = false;

    void Start()
    {
        // Tìm GameProgress
        gameProgress = GameProgress.Instance;
        if (gameProgress == null)
        {
            Debug.LogError("Không tìm thấy GameProgress.Instance!");

            // Thử tìm bằng cách khác
            gameProgress = FindObjectOfType<GameProgress>();
            if (gameProgress != null)
            {
                Debug.Log("Tìm thấy GameProgress qua FindObjectOfType");
            }
            else
            {
                Debug.LogError("Không tìm thấy GameProgress qua bất kỳ phương thức nào!");
                return;
            }
        }

        // Debug thông tin về độ dài mảng
        Debug.Log("Độ dài mảng GameProgress: Coins=" + gameProgress.requiredCoins.Length +
                  ", Chests=" + gameProgress.requiredChests.Length);

        // Lấy map hiện tại từ tên scene
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Map"))
        {
            string mapIndexStr = sceneName.Substring(3);
            if (int.TryParse(mapIndexStr, out int mapIndex))
            {
                currentMapIndex = mapIndex;
                Debug.Log("Đã phát hiện Map " + currentMapIndex + " từ tên scene: " + sceneName);

                // Kiểm tra giá trị mapIndex có hợp lệ không - ĐÃ SỬA ĐIỀU KIỆN CHO MAP 6
                if (currentMapIndex <= 0)
                {
                    Debug.LogError("currentMapIndex = " + currentMapIndex + " không hợp lệ (≤ 0)!");
                    currentMapIndex = 1; // Đặt về giá trị mặc định an toàn
                }
                else if (gameProgress.requiredCoins.Length <= currentMapIndex)
                {
                    Debug.LogError("currentMapIndex = " + currentMapIndex + " vượt quá độ dài mảng (" +
                                   gameProgress.requiredCoins.Length + ")!");
                    currentMapIndex = gameProgress.requiredCoins.Length - 1; // Đặt về map cao nhất có sẵn
                }

                // Cập nhật currentMap trong GameProgress
                gameProgress.SetCurrentMap(currentMapIndex);
                Debug.Log("Đã cập nhật currentMap trong GameProgress thành " + currentMapIndex);
            }
        }
        else
        {
            Debug.Log("Scene hiện tại không phải map chơi game: " + sceneName);
            // Nếu không phải map chơi game, ẩn UI
            if (missionPanel != null)
                missionPanel.SetActive(false);
            if (completionNotification != null)
                completionNotification.SetActive(false);
            if (winnerNotification != null)
                winnerNotification.SetActive(false);
            return;
        }

        // Thiết lập nút đóng
        if (closeButton != null)
            closeButton.onClick.AddListener(ToggleMissionPanel);

        // Ẩn thông báo hoàn thành và thông báo chiến thắng
        if (completionNotification != null)
            completionNotification.SetActive(false);
        if (winnerNotification != null)
            winnerNotification.SetActive(false);

        // Hiển thị panel nhiệm vụ nếu cần
        if (missionPanel != null)
        {
            missionPanel.SetActive(showOnStart);
            UpdateMissionInfo();
        }

        // Kiểm tra nếu map đã hoàn thành
        CheckCompletionStatus();

        // Đăng ký sự kiện Application.quitting để tránh lỗi null reference
        Application.quitting += OnApplicationQuitting;
    }

    void OnApplicationQuitting()
    {
        // Không làm gì đặc biệt, chỉ để ngăn lỗi khi thoát game
    }

    void OnDestroy()
    {
        Application.quitting -= OnApplicationQuitting;
    }

    void Update()
    {
        // Kiểm tra nếu không có GameProgress hoặc mapIndex không hợp lệ
        if (gameProgress == null || !IsMapIndexValid())
            return;

        // Bật/tắt panel khi nhấn phím
        if (Input.GetKeyDown(toggleMissionKey))
        {
            ToggleMissionPanel();
        }

        // Cập nhật thông tin nhiệm vụ theo interval để tránh quá nhiều cập nhật
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateMissionInfo();
            CheckCompletionStatus();
        }
    }

    // Kiểm tra mapIndex có hợp lệ không
    bool IsMapIndexValid()
    {
        if (currentMapIndex <= 0)
        {
            return false;
        }

        if (gameProgress == null)
        {
            return false;
        }

        if (gameProgress.collectedCoins == null || gameProgress.requiredCoins == null ||
            gameProgress.collectedChests == null || gameProgress.requiredChests == null)
        {
            return false;
        }

        // Điều kiện này đã được cải thiện để xử lý Map 6
        if (currentMapIndex >= gameProgress.collectedCoins.Length ||
            currentMapIndex >= gameProgress.requiredCoins.Length ||
            currentMapIndex >= gameProgress.collectedChests.Length ||
            currentMapIndex >= gameProgress.requiredChests.Length)
        {
            return false;
        }

        return true;
    }

    // Cập nhật thông tin nhiệm vụ - có thể gọi từ bên ngoài
    public void ForceUpdateMissionInfo()
    {
        UpdateMissionInfo();
        CheckCompletionStatus();
    }

    // Cập nhật thông tin nhiệm vụ
    void UpdateMissionInfo()
    {
        if (gameProgress == null || missionPanel == null)
            return;
        // Kiểm tra lại tính hợp lệ
        if (!IsMapIndexValid())
        {
            Debug.LogWarning("Không thể cập nhật thông tin nhiệm vụ - MapIndex không hợp lệ: " + currentMapIndex);
            return;
        }
        try
        {
            // Thiết lập tiêu đề
            if (missionTitle != null)
                missionTitle.text = "Nhiệm vụ Map " + currentMapIndex;
            // Hiển thị yêu cầu và tiến độ coins
            if (coinRequirementText != null)
                coinRequirementText.text = gameProgress.collectedCoins[currentMapIndex] + "/" + gameProgress.requiredCoins[currentMapIndex] + " Coins";
            // Hiển thị yêu cầu và tiến độ rương
            if (chestRequirementText != null)
                chestRequirementText.text = gameProgress.collectedChests[currentMapIndex] + "/" + gameProgress.requiredChests[currentMapIndex] + " Chests";
            // Hiển thị yêu cầu boss dưới dạng số lượng
            if (bossRequirementText != null)
            {
                int requiredBossCount = 0;
                int defeatedBossCount = 0;

                // Đếm tổng số boss cần đánh bại
                if (gameProgress.requireDeadKnight[currentMapIndex]) requiredBossCount++;
                if (gameProgress.requireAshe[currentMapIndex]) requiredBossCount++;
                if (gameProgress.requireZombie[currentMapIndex]) requiredBossCount++;

                // Đếm số boss đã đánh bại
                if (gameProgress.requireDeadKnight[currentMapIndex] && gameProgress.defeatedDeadKnight[currentMapIndex]) defeatedBossCount++;
                if (gameProgress.requireAshe[currentMapIndex] && gameProgress.defeatedAshe[currentMapIndex]) defeatedBossCount++;
                if (gameProgress.requireZombie[currentMapIndex] && gameProgress.defeatedZombie[currentMapIndex]) defeatedBossCount++;

                // Hiển thị số lượng boss
                if (requiredBossCount > 0)
                {
                    bossRequirementText.text = defeatedBossCount + "/" + requiredBossCount + " Bosses";
                    bossRequirementText.gameObject.SetActive(true);
                }
                else
                {
                    bossRequirementText.gameObject.SetActive(false);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi trong UpdateMissionInfo: " + e.Message);
        }
    }

    // Kiểm tra trạng thái hoàn thành map
    void CheckCompletionStatus()
    {
        if (gameProgress == null)
            return;

        // Kiểm tra lại tính hợp lệ
        if (!IsMapIndexValid())
        {
            Debug.LogWarning("Không thể kiểm tra hoàn thành - MapIndex không hợp lệ: " + currentMapIndex);
            return;
        }

        try
        {
            // Kiểm tra xem map đã hoàn thành chưa (đủ coins và chests)
            bool hasEnoughItems = gameProgress.HasEnoughItems(currentMapIndex);
            bool isCompleted = gameProgress.IsMapCompleted(currentMapIndex);

            if (Time.frameCount % 120 == 0) // Giảm tần suất log để tránh spam
            {
                string bossStatus = "";
                if (gameProgress.requireDeadKnight[currentMapIndex])
                {
                    bossStatus += " - DeadKnight: " + (gameProgress.defeatedDeadKnight[currentMapIndex] ? "✓" : "✗");
                }
                if (gameProgress.requireAshe[currentMapIndex])
                {
                    bossStatus += " - Ashe: " + (gameProgress.defeatedAshe[currentMapIndex] ? "✓" : "✗");
                }
                if (gameProgress.requireZombie[currentMapIndex])
                {
                    bossStatus += " - Zombie: " + (gameProgress.defeatedZombie[currentMapIndex] ? "✓" : "✗");
                }

                Debug.Log("Check completion: Map" + currentMapIndex +
                          " - Has enough items: " + hasEnoughItems +
                          " - Is completed: " + isCompleted +
                          " - Coins: " + gameProgress.collectedCoins[currentMapIndex] + "/" + gameProgress.requiredCoins[currentMapIndex] +
                          " - Chests: " + gameProgress.collectedChests[currentMapIndex] + "/" + gameProgress.requiredChests[currentMapIndex] +
                          bossStatus);
            }

            // Nếu vừa mới đủ điều kiện hoàn thành và chưa hiển thị thông báo
            if (hasEnoughItems && !isCompleted && !hasShownCompletionNotification)
            {
                // Đánh dấu map đã hoàn thành
                gameProgress.CompleteMap(currentMapIndex);

                // Đánh dấu đã hiển thị thông báo
                hasShownCompletionNotification = true;

                // Hiển thị thông báo dựa trên map hiện tại
                if (currentMapIndex == 6) // Nếu là Map 6 (map cuối)
                {
                    ShowWinnerNotification();
                }
                else
                {
                    // Hiển thị thông báo thông thường cho các map khác
                    ShowCompletionNotification();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi trong CheckCompletionStatus: " + e.Message + "\nStackTrace: " + e.StackTrace);
        }
    }

    // Thêm phương thức hiển thị thông báo chiến thắng game
    void ShowWinnerNotification()
    {
        if (winnerNotification == null)
        {
            Debug.LogError("WinnerNotification chưa được gán!");

            // Nếu không có thông báo chiến thắng, hiển thị thông báo hoàn thành thông thường
            ShowCompletionNotification();
            return;
        }

        Debug.Log("Hiển thị thông báo CHIẾN THẮNG GAME!");

        // Ẩn thông báo hoàn thành thông thường nếu đang hiển thị
        if (completionNotification != null && completionNotification.activeSelf)
        {
            completionNotification.SetActive(false);
        }

        // Hiển thị thông báo chiến thắng
        winnerNotification.SetActive(true);

        // Đảm bảo nó nằm trên cùng
        Canvas canvas = winnerNotification.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            winnerNotification.transform.SetAsLastSibling();
        }

        // Thiết lập text nếu cần
        if (winnerText != null)
        {
            winnerText.text = "Bạn đã chiến thắng !!";
        }

        // Không tự động ẩn thông báo chiến thắng, người chơi phải nhấn nút Reset hoặc Exit
    }

    // Thêm phương thức này vào MissionDisplay.cs
    public void ForceShowCompletionPanel()
    {
        if (completionNotification != null)
        {
            completionNotification.SetActive(true);

            if (completionText != null)
            {
                // Kiểm tra xem có phải là map cuối không
                if (currentMapIndex == 6)
                {
                    completionText.text = "Đã hoàn thành Map " + currentMapIndex + "!\nBạn đã chiến thắng game!";
                }
                else
                {
                    completionText.text = "Đã hoàn thành Map " + currentMapIndex + "!\nĐã mở khóa Map " + (currentMapIndex + 1);
                }
            }

            Debug.Log("Force hiển thị panel hoàn thành");
        }
        else
        {
            Debug.LogError("Completion Notification chưa được gán!");
        }
    }

    void ShowCompletionNotification()
    {
        if (completionNotification == null)
        {
            Debug.LogError("CompletionNotification chưa được gán!");
            return;
        }

        Debug.Log("Hiển thị thông báo hoàn thành nhiệm vụ!");

        // Đảm bảo completionNotification được kích hoạt
        completionNotification.SetActive(true);

        // Đảm bảo nó nằm trên cùng
        Canvas canvas = completionNotification.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // Di chuyển thông báo lên trên cùng trong canvas
            completionNotification.transform.SetAsLastSibling();
        }

        if (completionText != null)
        {
            // Kiểm tra xem có phải là map cuối không
            if (currentMapIndex == 6)
            {
                completionText.text = "Đã hoàn thành Map " + currentMapIndex + "!\nBạn đã chiến thắng game!";
            }
            else
            {
                completionText.text = "Đã hoàn thành Map " + currentMapIndex + "!\nĐã mở khóa Map " + (currentMapIndex + 1);
            }
        }

        // Tự động ẩn sau một khoảng thời gian
        StopAllCoroutines(); // Đảm bảo không có coroutine cũ
        StartCoroutine(HideNotificationAfterDelay());
    }

    // Ẩn thông báo sau delay
    IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);

        if (completionNotification != null && completionNotification.activeSelf)
            completionNotification.SetActive(false);
    }

    // Bật/tắt panel nhiệm vụ
    public void ToggleMissionPanel()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(!missionPanel.activeSelf);

            if (missionPanel.activeSelf)
            {
                UpdateMissionInfo();
            }
        }
    }

    // Thêm phương thức để reset lại flag đã hiển thị thông báo
    public void ResetCompletionFlag()
    {
        hasShownCompletionNotification = false;
    }
}
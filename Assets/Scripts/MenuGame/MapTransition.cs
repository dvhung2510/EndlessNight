using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MapTransition : MonoBehaviour
{
    [Header("Map Settings")]
    [Tooltip("Index của map hiện tại (0=HomeScene, 1-6=Map)")]
    public int currentMapIndex = 1;

    [Tooltip("Tên scene sẽ tải khi hoàn thành map hiện tại")]
    public string nextMapScene = "";

    [Tooltip("Tên scene chọn map")]
    public string mapSelectionScene = "MapSelection";

    [Tooltip("Tải map tiếp theo trực tiếp thay vì về menu")]
    public bool loadNextMapDirectly = true;

    [Header("Spawn Settings")]
    [Tooltip("Vị trí xuất hiện trong map tiếp theo (X)")]
    public float spawnPositionX = 0f;

    [Tooltip("Vị trí xuất hiện trong map tiếp theo (Y)")]
    public float spawnPositionY = 0f;

    [Header("Hiệu ứng và âm thanh")]
    public GameObject successEffect;
    public GameObject failedEffect;
    public AudioClip successSound;
    public AudioClip failedSound;
    private AudioSource audioSource;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDistance = 2f;

    [Header("UI Elements")]
    public GameObject completionPanel;
    public GameObject missionFailedPanel;
    public TextMeshProUGUI failedMessageText;
    public float messageDisplayTime = 3f;

    [Header("Scene Names")]
    public string[] mapSceneNames;
    public string menuSceneName = "HomeScene";

    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool skipMapRequirements = false;

    private bool isTransitioning = false;
    private GameProgress gameProgress;

    [Header("Loading UI")]
    public GameObject loadingScreen;  // Canvas loading screen
    public Image loadingBar;         // Thanh tiến trình
    public TextMeshProUGUI loadingBarText; // Text hiển thị tiến trình

    // Dictionary ánh xạ tên scene thành index map
    private readonly Dictionary<string, int> sceneNameToMapIndex = new Dictionary<string, int>() {
        {"HomeScene", 0},
        {"Map1", 1},
        {"Map2", 2},
        {"Map3", 3},
        {"Map4", 4},
        {"Map5", 5},
        {"Map6", 6}
    };

    // Dictionary ánh xạ index map thành tên scene
    private readonly Dictionary<int, string> mapIndexToSceneName = new Dictionary<int, string>() {
        {0, "HomeScene"},
        {1, "Map1"},
        {2, "Map2"},
        {3, "Map3"},
        {4, "Map4"},
        {5, "Map5"},
        {6, "Map6"}
    };

    private void Start()
    {
        // Tự động phát hiện map hiện tại dựa trên tên scene
        AutoDetectCurrentMap();

        // Tìm GameProgress component
        gameProgress = GameProgress.Instance;
        if (gameProgress == null)
        {
            // Tạo GameProgress nếu chưa có
            GameObject progressObj = new GameObject("GameProgress");
            gameProgress = progressObj.AddComponent<GameProgress>();
            DontDestroyOnLoad(progressObj);

            LogDebug("Đã tạo GameProgress mới do không tìm thấy!");
        }

        // Đồng bộ map hiện tại với GameProgress
        SyncCurrentMapWithGameProgress();

        // Tìm hoặc tạo AudioSource nếu cần
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (successSound != null || failedSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ẩn các panel UI
        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (missionFailedPanel != null)
            missionFailedPanel.SetActive(false);

        // Ẩn các hiệu ứng
        if (successEffect != null)
            successEffect.SetActive(false);

        if (failedEffect != null)
            failedEffect.SetActive(false);

        // Nếu mapSceneNames trống, tự động tạo mảng với tên mặc định
        if (mapSceneNames == null || mapSceneNames.Length == 0)
        {
            mapSceneNames = new string[7]; // 0 không dùng, 1-6 cho Map1-Map6
            for (int i = 1; i < mapSceneNames.Length; i++)
            {
                mapSceneNames[i] = "Map" + i;
            }
        }

        // Kiểm tra xem map có được mở khóa không (bỏ qua HomeScene và chế độ debug)
        if (currentMapIndex > 0 && !skipMapRequirements && !gameProgress.IsMapUnlocked(currentMapIndex))
        {
            LogDebug("Map " + currentMapIndex + " chưa được mở khóa! Quay về màn hình chọn map.");

            // Nếu map chưa được mở khóa, quay về HomeScene 
            SceneManager.LoadScene("HomeScene");
            return;
        }

        // Đặt vị trí xuất hiện của Player nếu có
        CheckCustomSpawnPosition();

        if (loadingScreen != null)
        {
            DontDestroyOnLoad(loadingScreen);
            loadingScreen.SetActive(false); // Ẩn ban đầu
        }
    }

    // Tự động phát hiện map hiện tại dựa trên tên scene
    private void AutoDetectCurrentMap()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LogDebug("Tự động phát hiện map từ scene: " + currentSceneName);

        // Tìm trong dictionary
        if (sceneNameToMapIndex.TryGetValue(currentSceneName, out int mapIndex))
        {
            currentMapIndex = mapIndex;
            LogDebug("Tự động phát hiện: Đây là Map " + mapIndex);
        }
        else if (currentSceneName.StartsWith("Map"))
        {
            // Cố gắng trích xuất số map từ tên nếu không tìm thấy trong dictionary
            string mapIndexStr = currentSceneName.Substring(3);
            if (int.TryParse(mapIndexStr, out int extractedIndex))
            {
                currentMapIndex = extractedIndex;
                LogDebug("Tự động phát hiện từ tên scene: Đây là Map " + extractedIndex);
            }
        }
        else if (currentSceneName.Contains("Home"))
        {
            currentMapIndex = 0;
            LogDebug("Tự động phát hiện từ tên scene: Đây là HomeScene");
        }
        else
        {
            LogDebug("Không thể tự động phát hiện map từ tên scene: " + currentSceneName);
        }
    }

    // Đồng bộ currentMapIndex với giá trị trong GameProgress
    private void SyncCurrentMapWithGameProgress()
    {
        if (gameProgress != null)
        {
            // Cập nhật currentMapIndex trong GameProgress
            if (currentMapIndex > 0)
            {
                gameProgress.SetCurrentMap(currentMapIndex);
                LogDebug("Đã đồng bộ currentMapIndex từ MapTransition (" + currentMapIndex + ") đến GameProgress");
            }
            else
            {
                // Nếu đang ở HomeScene, không cần cập nhật GameProgress
                LogDebug("Đang ở HomeScene (index 0), không cần cập nhật GameProgress");
            }

            // Đảm bảo GameProgress cập nhật từ tên scene
            gameProgress.UpdateCurrentMapFromSceneName();
        }
    }

    // Kiểm tra và đặt vị trí xuất hiện tùy chỉnh
    private void CheckCustomSpawnPosition()
    {
        if (PlayerPrefs.GetInt("UseCustomSpawn", 0) == 1)
        {
            float posX = PlayerPrefs.GetFloat("SpawnPositionX", 0);
            float posY = PlayerPrefs.GetFloat("SpawnPositionY", 0);

            // Tìm Player và đặt vị trí
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(posX, posY, player.transform.position.z);
                LogDebug("Đã đặt vị trí Player tại: " + posX + ", " + posY);
            }
            else
            {
                LogDebug("Không tìm thấy Player để đặt vị trí spawn!");
            }

            // Reset để không sử dụng lại trong lần tải tiếp theo
            PlayerPrefs.SetInt("UseCustomSpawn", 0);
            PlayerPrefs.Save();
        }
    }

    // Ghi log debug
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log("[MapTransition] " + message);
        }
    }

    // Xử lý khi Player va chạm với cổng chuyển map
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTransitioning) return;

        // Kiểm tra nếu đó là Player
        if (collision.CompareTag("Player"))
        {
            LogDebug("Player đã va chạm với cổng chuyển map!");
            HandlePlayerReachGoal(collision.gameObject);
        }
    }

    // Xử lý khi người chơi đến đích (kết hợp từ GoalPost)
    private void HandlePlayerReachGoal(GameObject player)
    {
        // Nếu là HomeScene, chuyển đến map tiếp theo mà không cần kiểm tra
        if (currentMapIndex == 0)
        {
            TransitionToNextMap();
            return;
        }

        // Nếu ở chế độ skipMapRequirements, bỏ qua kiểm tra hoàn thành map
        if (skipMapRequirements)
        {
            LogDebug("Bỏ qua kiểm tra yêu cầu hoàn thành map (skipMapRequirements=true)");
            TransitionToNextMap();
            return;
        }

        // Kiểm tra xem đã hoàn thành nhiệm vụ chưa
        bool objectivesCompleted = gameProgress.HasEnoughItems(currentMapIndex);

        // In ra các giá trị để debug
        LogDebug("Xu đã thu thập: " + gameProgress.collectedCoins[currentMapIndex] +
                "/" + gameProgress.requiredCoins[currentMapIndex]);
        LogDebug("Rương đã thu thập: " + gameProgress.collectedChests[currentMapIndex] +
                "/" + gameProgress.requiredChests[currentMapIndex]);
        LogDebug("Nhiệm vụ hoàn thành? " + objectivesCompleted);

        if (objectivesCompleted)
        {
            // ĐÃ HOÀN THÀNH NHIỆM VỤ
            StartCoroutine(HandleMapCompletion(currentMapIndex, player));
        }
        else
        {
            // CHƯA HOÀN THÀNH NHIỆM VỤ
            StartCoroutine(HandleMissionFailed(currentMapIndex, player));
        }
    }

    // Xử lý khi hoàn thành map (kết hợp từ GoalPost)
    private IEnumerator HandleMapCompletion(int mapIndex, GameObject player)
    {
        // Phát âm thanh thành công
        if (audioSource != null && successSound != null)
        {
            audioSource.clip = successSound;
            audioSource.Play();
        }

        // Hiển thị hiệu ứng thành công
        if (successEffect != null)
        {
            successEffect.SetActive(true);
        }

        // Hiển thị panel hoàn thành nếu có
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }

        // Vô hiệu hóa điều khiển người chơi (để tránh người chơi di chuyển)
        DisablePlayerControl(player, true);

        // Đánh dấu map đã hoàn thành (tự động mở khóa map tiếp theo)
        gameProgress.CompleteMap(mapIndex);
        LogDebug("Đã đánh dấu Map " + mapIndex + " là hoàn thành!");

        // Đợi một khoảng thời gian để hiển thị hiệu ứng
        yield return new WaitForSeconds(2f);

        // Quyết định xem có tải trực tiếp map tiếp theo hay quay về menu
        if (loadNextMapDirectly)
        {
            LoadNextMap(mapIndex);
        }
        else
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }

    private void LoadNextMap(int currentMapIndex)
    {
        isTransitioning = true;
        int nextMapIndex = currentMapIndex + 1;

        // Lưu vị trí xuất hiện cho map tiếp theo
        PlayerPrefs.SetFloat("SpawnPositionX", spawnPositionX);
        PlayerPrefs.SetFloat("SpawnPositionY", spawnPositionY);
        PlayerPrefs.SetInt("UseCustomSpawn", 1);
        PlayerPrefs.Save();

        LogDebug("=== BẮT ĐẦU XỬ LÝ CHUYỂN MAP ===");
        LogDebug("Map hiện tại: " + currentMapIndex + ", Map tiếp theo: " + nextMapIndex);
        LogDebug("Đã lưu vị trí spawn: " + spawnPositionX + ", " + spawnPositionY);

        // Force set current map
        gameProgress.SetCurrentMap(nextMapIndex);
        LogDebug("Đã cập nhật currentMap = " + nextMapIndex);

        // Tên scene mặc định
        string defaultSceneName = "Map" + nextMapIndex;

        // Tên scene từ mảng nếu có
        string configuredSceneName = "";
        if (mapSceneNames != null && nextMapIndex < mapSceneNames.Length)
        {
            configuredSceneName = mapSceneNames[nextMapIndex];
        }

        // Sử dụng tên scene từ mảng nếu có, nếu không dùng tên mặc định
        string sceneToLoad = !string.IsNullOrEmpty(configuredSceneName) ? configuredSceneName : defaultSceneName;
        LogDebug("Chuẩn bị tải scene: " + sceneToLoad);

        try
        {
            // Bắt đầu quá trình tải bất đồng bộ
            StartCoroutine(LoadSceneAsync(sceneToLoad));
        }
        catch (System.Exception e)
        {
            Debug.LogError("LỖI KHI TẢI SCENE: " + e.Message);

            // Nếu lỗi, tải HomeScene
            StartCoroutine(LoadSceneAsync(menuSceneName));
        }
    }
    // Xử lý khi nhiệm vụ chưa hoàn thành (kết hợp từ GoalPost)
    private IEnumerator HandleMissionFailed(int mapIndex, GameObject player)
    {
        // Phát âm thanh thất bại
        if (audioSource != null && failedSound != null)
        {
            audioSource.clip = failedSound;
            audioSource.Play();
        }

        // Hiển thị hiệu ứng thất bại
        if (failedEffect != null)
        {
            failedEffect.SetActive(true);
            // Ẩn hiệu ứng sau 1 giây
            StartCoroutine(DeactivateAfterDelay(failedEffect, 1f));
        }

        // Hiển thị thông báo nhiệm vụ chưa hoàn thành
        ShowMissionFailedMessage(mapIndex);

        // Đẩy lùi người chơi
        KnockbackPlayer(player);

        // Đợi một khoảng thời gian trước khi ẩn thông báo
        yield return new WaitForSeconds(messageDisplayTime);

        // Ẩn thông báo
        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }
    }

    // Hiển thị thông báo khi nhiệm vụ chưa hoàn thành (kết hợp từ GoalPost)
    private void ShowMissionFailedMessage(int mapIndex)
    {
        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(true);

            // Cập nhật nội dung thông báo chi tiết
            if (failedMessageText != null)
            {
                string message = "Nhiệm vụ chưa hoàn thành:\n";

                // Kiểm tra xu
                int collectedCoins = gameProgress.collectedCoins[mapIndex];
                int requiredCoins = gameProgress.requiredCoins[mapIndex];
                if (collectedCoins < requiredCoins)
                {
                    message += "- Cần thu thập thêm " + (requiredCoins - collectedCoins) + " xu\n";
                }

                // Kiểm tra rương
                int collectedChests = gameProgress.collectedChests[mapIndex];
                int requiredChests = gameProgress.requiredChests[mapIndex];
                if (collectedChests < requiredChests)
                {
                    message += "- Cần tìm thêm " + (requiredChests - collectedChests) + " rương\n";
                }

                // Kiểm tra boss (nếu cần)
                if (gameProgress.requireDeadKnight[mapIndex] && !gameProgress.defeatedDeadKnight[mapIndex])
                {
                    message += "- Cần đánh bại DeadKnight\n";
                }

                if (gameProgress.requireZombie[mapIndex] && !gameProgress.defeatedZombie[mapIndex])
                {
                    message += "- Cần đánh bại Zombie\n";
                }

                if (gameProgress.requireAshe[mapIndex] && !gameProgress.defeatedAshe[mapIndex])
                {
                    message += "- Cần đánh bại Ashe\n";
                }

                failedMessageText.text = message;
            }
        }
    }

    // Đẩy lùi người chơi khi chưa hoàn thành nhiệm vụ (từ GoalPost)
    private void KnockbackPlayer(GameObject player)
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            // Tính hướng đẩy lùi (ngược với hướng của cổng đích)
            Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
            // Reset vận tốc trước khi đẩy
            playerRb.linearVelocity = Vector2.zero;
            // Đẩy lùi người chơi
            playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
        else
        {
            // Nếu không có Rigidbody2D, di chuyển trực tiếp
            Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
            player.transform.position += (Vector3)knockbackDirection * knockbackDistance;
        }
    }

    // Vô hiệu hóa điều khiển của người chơi (từ GoalPost)
    private void DisablePlayerControl(GameObject player, bool disable)
    {
        // Tìm script điều khiển người chơi (thử các tên script phổ biến)
        MonoBehaviour[] possibleControllers = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour controller in possibleControllers)
        {
            string controllerName = controller.GetType().Name.ToLower();
            if (controllerName.Contains("controller") || controllerName.Contains("movement") ||
                controllerName.Contains("player") || controllerName.Contains("character"))
            {
                controller.enabled = !disable;
            }
        }

        // Nếu có Rigidbody2D, có thể đặt chế độ kinematic
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = !disable;
        }
    }

    // Đóng panel thông báo nhiệm vụ thất bại
    public void CloseMissionFailedPanel()
    {
        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }
    }

    // Hàm trợ giúp để vô hiệu hóa GameObject sau một khoảng thời gian
    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }

    // Chuyển đến map tiếp theo (cho UI button)
    public void TransitionToNextMap()
    {
        if (isTransitioning) return;

        int nextMapIndex = currentMapIndex + 1;
        if (nextMapIndex <= 6) // Kiểm tra xem có vượt quá số map không
        {
            LoadNextMap(currentMapIndex);
        }
        else
        {
            // Nếu là map cuối cùng, quay về HomeScene
            ReturnToHome();
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        LogDebug("Bắt đầu tải scene bất đồng bộ: " + sceneName);

        // Hiển thị màn hình loading nếu có
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Cập nhật UI nếu có
        if (loadingBarText != null)
            loadingBarText.text = "Đang tải... 0%";

        if (loadingBar != null)
        {
            // Đặt scale.x ban đầu = 0
            Vector3 scale = loadingBar.transform.localScale;
            scale.x = 0;
            loadingBar.transform.localScale = scale;
        }

        // Tải scene bất đồng bộ
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Không cho phép kích hoạt scene ngay lập tức
        asyncLoad.allowSceneActivation = false;

        // Đợi đến khi tải được ít nhất 90%
        float progress = 0;
        while (asyncLoad.progress < 0.9f)
        {
            // Tính toán tiến trình (0->1)
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Cập nhật thanh tiến trình bằng cách thay đổi scale
            if (loadingBar != null)
            {
                Vector3 scale = loadingBar.transform.localScale;
                scale.x = progress;
                loadingBar.transform.localScale = scale;
            }

            // Cập nhật text
            if (loadingBarText != null)
                loadingBarText.text = "Đang tải... " + Mathf.Floor(progress * 100).ToString() + "%";

            LogDebug("Tiến độ tải: " + (progress * 100).ToString("F0") + "%");

            yield return null;
        }

        // Đợi thêm một khoảng thời gian để tránh hiện tượng nhấp nháy
        yield return new WaitForSeconds(0.5f);

        // Hoàn tất scale
        if (loadingBar != null)
        {
            Vector3 scale = loadingBar.transform.localScale;
            scale.x = 1;
            loadingBar.transform.localScale = scale;
        }

        // Cập nhật text
        if (loadingBarText != null)
            loadingBarText.text = "Đang tải... 100%";

        // Đợi thêm 0.2 giây
        yield return new WaitForSeconds(0.2f);

        // Hoàn tất tải và kích hoạt scene
        asyncLoad.allowSceneActivation = true;

        // Đợi đến khi scene đã được kích hoạt hoàn toàn
        yield return new WaitForSeconds(0.5f);

        // ẨN LOADING SCREEN SAU KHI SCENE MỚI ĐÃ ĐƯỢC KÍCH HOẠT
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        LogDebug("Tải scene hoàn tất: " + sceneName);
        isTransitioning = false;
    }
    // Quay về HomeScene
    public void ReturnToHome()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        LogDebug("Quay về HomeScene");
        StartCoroutine(LoadSceneAsync(menuSceneName));
    }
}
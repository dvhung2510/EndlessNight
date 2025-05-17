using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
public class GoalPost : MonoBehaviour
{
    [Header("Cài đặt cơ bản")]
    public float knockbackForce = 5f; // Lực đẩy lùi người chơi khi chưa hoàn thành nhiệm vụ
    public float knockbackDistance = 2f; // Khoảng cách đẩy lùi
    public bool loadNextMapDirectly = true; // Tùy chọn để tải trực tiếp map tiếp theo
    [Header("Thông báo")]
    public GameObject missionFailedPanel; // Panel thông báo nhiệm vụ chưa hoàn thành
    public TextMeshProUGUI failedMessageText; // Văn bản thông báo chi tiết
    public float messageDisplayTime = 3f; // Thời gian hiển thị thông báo
    [Header("Hiệu ứng")]
    public GameObject successEffect; // Hiệu ứng khi hoàn thành nhiệm vụ
    public GameObject failedEffect; // Hiệu ứng khi chưa hoàn thành nhiệm vụ
    public AudioClip successSound; // Âm thanh khi hoàn thành
    public AudioClip failedSound; // Âm thanh khi thất bại
    [Header("Scene Names")]
    public string[] mapSceneNames; // Tên các scene map (Map1, Map2, ...)
    public string menuSceneName = "MenuScene"; // Tên scene menu
    private AudioSource audioSource;
    private void Start()
    {
        // Tìm hoặc tạo AudioSource nếu cần
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (successSound != null || failedSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Ẩn các panel thông báo khi bắt đầu
        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(false);
        }
        // Ẩn các hiệu ứng khi bắt đầu
        if (successEffect != null) successEffect.SetActive(false);
        if (failedEffect != null) failedEffect.SetActive(false);
        // Nếu mapSceneNames trống, tự động tạo mảng với tên mặc định
        if (mapSceneNames == null || mapSceneNames.Length == 0)
        {
            mapSceneNames = new string[7]; // 0 không dùng, 1-6 cho Map1-Map6
            for (int i = 1; i < mapSceneNames.Length; i++)
            {
                mapSceneNames[i] = "Map" + i;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerReachGoal(other.gameObject);
        }
    }
    // Xử lý khi người chơi đến đích
    private void HandlePlayerReachGoal(GameObject player)
    {
        // Lấy map hiện tại từ GameProgress
        int currentMap = GameProgress.Instance.currentMap;
        Debug.Log("Map hiện tại: " + currentMap);
        // Kiểm tra xem đã hoàn thành nhiệm vụ chưa
        bool objectivesCompleted = GameProgress.Instance.HasEnoughItems(currentMap);
        // In ra các giá trị để debug
        Debug.Log("Xu đã thu thập: " + GameProgress.Instance.collectedCoins[currentMap] +
                  "/" + GameProgress.Instance.requiredCoins[currentMap]);
        Debug.Log("Rương đã thu thập: " + GameProgress.Instance.collectedChests[currentMap] +
                  "/" + GameProgress.Instance.requiredChests[currentMap]);
        Debug.Log("Nhiệm vụ hoàn thành? " + objectivesCompleted);
        if (objectivesCompleted)
        {
            // ĐÃ HOÀN THÀNH NHIỆM VỤ
            StartCoroutine(HandleMapCompletion(currentMap, player));
        }
        else
        {
            // CHƯA HOÀN THÀNH NHIỆM VỤ
            StartCoroutine(HandleMissionFailed(currentMap, player));
        }
    }
    // Xử lý khi hoàn thành map
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
        // Vô hiệu hóa điều khiển người chơi (để tránh người chơi di chuyển)
        DisablePlayerControl(player, true);
        // Đánh dấu map đã hoàn thành (tự động mở khóa map tiếp theo)
        GameProgress.Instance.CompleteMap(mapIndex);
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
    // Tải map tiếp theo
    private void LoadNextMap(int currentMapIndex)
    {
        int nextMapIndex = currentMapIndex + 1;
        Debug.Log("=== BẮT ĐẦU XỬ LÝ CHUYỂN MAP ===");
        Debug.Log("Map hiện tại: " + currentMapIndex + ", Map tiếp theo: " + nextMapIndex);
        // Force set current map
        GameProgress.Instance.SetCurrentMap(nextMapIndex);
        Debug.Log("Đã cập nhật currentMap = " + nextMapIndex);
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
        Debug.Log("Chuẩn bị tải scene: " + sceneToLoad);
        // Hiển thị tất cả scene đã đăng ký
        Debug.Log("Danh sách scene đã đăng ký:");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log(" - " + i + ": " + sceneName);
        }
        try
        {
            Debug.Log("ĐANG TẢI SCENE: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        catch (System.Exception e)
        {
            Debug.LogError("LỖI KHI TẢI SCENE: " + e.Message);
            // Thử tải bằng index
            try
            {
                Debug.Log("Thử tải scene bằng index: " + nextMapIndex);
                SceneManager.LoadScene(nextMapIndex);
            }
            catch (System.Exception e2)
            {
                Debug.LogError("LỖI KHI TẢI SCENE BẰNG INDEX: " + e2.Message);
                SceneManager.LoadScene(menuSceneName);
            }
        }
    }
    // Xử lý khi nhiệm vụ chưa hoàn thành
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
    // Hiển thị thông báo khi nhiệm vụ chưa hoàn thành
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
                int collectedCoins = GameProgress.Instance.collectedCoins[mapIndex];
                int requiredCoins = GameProgress.Instance.requiredCoins[mapIndex];
                if (collectedCoins < requiredCoins)
                {
                    message += "- Cần thu thập thêm " + (requiredCoins - collectedCoins) + " xu\n";
                }
                // Kiểm tra rương
                int collectedChests = GameProgress.Instance.collectedChests[mapIndex];
                int requiredChests = GameProgress.Instance.requiredChests[mapIndex];
                if (collectedChests < requiredChests)
                {
                    message += "- Cần tìm thêm " + (requiredChests - collectedChests) + " rương\n";
                }
                // Kiểm tra boss (nếu cần)
                if (GameProgress.Instance.requireDeadKnight[mapIndex] && !GameProgress.Instance.defeatedDeadKnight[mapIndex])
                {
                    message += "- Cần đánh bại DeadKnight\n";
                }
                if (GameProgress.Instance.requireZombie[mapIndex] && !GameProgress.Instance.defeatedZombie[mapIndex])
                {
                    message += "- Cần đánh bại Zombie\n";
                }
                if (GameProgress.Instance.requireAshe[mapIndex] && !GameProgress.Instance.defeatedAshe[mapIndex])
                {
                    message += "- Cần đánh bại Ashe\n";
                }
                failedMessageText.text = message;
            }
        }
    }
    // Đẩy lùi người chơi khi chưa hoàn thành nhiệm vụ
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
            // Có thể thêm animation knockback tại đây
        }
        else
        {
            // Nếu không có Rigidbody2D, di chuyển trực tiếp
            Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
            player.transform.position += (Vector3)knockbackDirection * knockbackDistance;
        }
    }
    // Vô hiệu hóa điều khiển của người chơi
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
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MapSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mapSelectionPanel;     // GameObject chứa toàn bộ giao diện chọn map
    public GameObject mainMenuPanel;         // GameObject chứa toàn bộ menu chính
    public GameObject messagePanel;          // Panel hiển thị thông báo
    public TextMeshProUGUI messageText;      // Text hiển thị nội dung thông báo

    [Header("Maps Configuration")]
    public List<MapConfig> maps = new List<MapConfig>();   // Danh sách cấu hình các map
    public List<Button> mapButtons = new List<Button>();   // Danh sách các nút map

    [Header("Map Objectives Text")]
    public List<TextMeshProUGUI> coinTexts = new List<TextMeshProUGUI>();   // Text hiển thị coin
    public List<TextMeshProUGUI> chestTexts = new List<TextMeshProUGUI>();  // Text hiển thị chest 
    public List<TextMeshProUGUI> bossTexts = new List<TextMeshProUGUI>();   // Text hiển thị boss

    // Debug Mode - Cho phép load map mà không cần scene thật
    public bool debugMode = true;

    // PlayerPrefs Keys
    private const string UNLOCKED_MAPS_KEY = "UnlockedMaps";
    private const string COLLECTED_COINS_KEY = "CollectedCoins_";
    private const string COLLECTED_CHESTS_KEY = "CollectedChests_";
    private const string DEFEATED_DEADKNIGHT_KEY = "DefeatedDeadKnight_";
    private const string DEFEATED_ASHE_KEY = "DefeatedAshe_";
    private const string DEFEATED_ZOMBIE_KEY = "DefeatedZombie_";

    private void Start()
    {
        // Kiểm tra và sửa chữa dữ liệu không hợp lệ
        ValidatePlayerPrefs();

        InitializeMapSelection();
        UpdateMapUI();
    }

    // Kiểm tra và sửa dữ liệu PlayerPrefs không hợp lệ
    private void ValidatePlayerPrefs()
    {
        // Kiểm tra giá trị UnlockedMaps
        int unlockedMaps = PlayerPrefs.GetInt(UNLOCKED_MAPS_KEY, 1);
        if (unlockedMaps <= 0 || unlockedMaps > maps.Count)
        {
            Debug.LogWarning("Phát hiện giá trị Maps đã mở khóa không hợp lệ: " + unlockedMaps + ". Đặt lại về 1.");
            unlockedMaps = 1;
            PlayerPrefs.SetInt(UNLOCKED_MAPS_KEY, unlockedMaps);
        }

        // Kiểm tra giá trị CurrentMap
        int currentMap = PlayerPrefs.GetInt("CurrentMap", 0);
        if (currentMap < 0 || currentMap >= maps.Count)
        {
            Debug.LogWarning("Phát hiện giá trị CurrentMap không hợp lệ: " + currentMap + ". Đặt lại về 0.");
            PlayerPrefs.SetInt("CurrentMap", 0);
        }

        // Đảm bảo lưu thay đổi
        PlayerPrefs.Save();
    }

    public void ShowMapSelection()
    {
        Debug.Log("MapSelectionManager: Đang hiển thị màn hình chọn map");

        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(true);
        }

        // Không cần ẩn mainMenuPanel ở đây vì MainMenuController đã xử lý

        // Cập nhật UI map
        UpdateMapUI();

        // In ra log debug để kiểm tra xem đã mở khóa bao nhiêu map
        DebugPlayerPrefs();
    }

    public void HideMapSelection()
    {
        Debug.Log("MapSelectionManager: Đang ẩn màn hình chọn map");

        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    // Thêm phương thức debug
    public void DebugPlayerPrefs()
    {
        int unlockedMaps = PlayerPrefs.GetInt(UNLOCKED_MAPS_KEY, 1);
        int currentMap = PlayerPrefs.GetInt("CurrentMap", 0);

        Debug.Log("======== DEBUG INFO ========");
        Debug.Log("Maps đã mở khóa: " + unlockedMaps);
        Debug.Log("Map hiện tại: " + currentMap);

        for (int i = 0; i < maps.Count; i++)
        {
            Debug.Log("Map " + (i + 1) + ":");
            Debug.Log("  - Coins: " + PlayerPrefs.GetInt(COLLECTED_COINS_KEY + i, 0) + "/" + maps[i].requiredCoins);
            Debug.Log("  - Chests: " + PlayerPrefs.GetInt(COLLECTED_CHESTS_KEY + i, 0) + "/" + maps[i].requiredChests);
            Debug.Log("  - DeadKnight: " + (PlayerPrefs.GetInt(DEFEATED_DEADKNIGHT_KEY + i, 0) > 0 ? "Đã đánh bại" : "Chưa đánh bại"));
            Debug.Log("  - Ashe: " + (PlayerPrefs.GetInt(DEFEATED_ASHE_KEY + i, 0) > 0 ? "Đã đánh bại" : "Chưa đánh bại"));
            Debug.Log("  - Zombie: " + (PlayerPrefs.GetInt(DEFEATED_ZOMBIE_KEY + i, 0) > 0 ? "Đã đánh bại" : "Chưa đánh bại"));
        }
    }

    public void LoadMap(int mapIndex)
    {
        Debug.Log("===============================");
        Debug.Log("Đang cố gắng tải map: " + (mapIndex + 1));

        // Kiểm tra xem map đã được mở khóa chưa
        int unlockedMaps = PlayerPrefs.GetInt(UNLOCKED_MAPS_KEY, 1);

        // Kiểm tra giá trị không hợp lệ và tự động sửa
        if (unlockedMaps <= 0 || unlockedMaps > maps.Count)
        {
            Debug.LogWarning("Phát hiện giá trị Maps đã mở khóa không hợp lệ: " + unlockedMaps + ". Đặt lại về 1.");
            unlockedMaps = 1;
            PlayerPrefs.SetInt(UNLOCKED_MAPS_KEY, unlockedMaps);
            PlayerPrefs.Save();
        }

        Debug.Log("Maps đã mở khóa: " + unlockedMaps + ", map đang chọn: " + (mapIndex + 1));

        if (mapIndex < unlockedMaps)
        {
            // Lưu map hiện tại đang chơi
            PlayerPrefs.SetInt("CurrentMap", mapIndex);
            PlayerPrefs.Save();
            Debug.Log("Đã lưu CurrentMap = " + mapIndex);

            // Chế độ debug - không cần scene thật
            if (debugMode)
            {
                Debug.Log("DEBUG MODE: Giả lập tải map " + (mapIndex + 1));
                ShowMessage("DEBUG MODE: Đang chơi map " + (mapIndex + 1), 3f); // Hiển thị lâu hơn

                // Giả lập hoàn thành map và mở khóa map tiếp theo (nhấn Shift)
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    Debug.Log("Đang giả lập hoàn thành map " + (mapIndex + 1));
                    // Hoàn thành tất cả nhiệm vụ
                    SimulateCompleteMissions(mapIndex);

                    // Mở khóa map tiếp theo
                    UnlockNextMap(mapIndex);
                    UpdateMapUI();
                    ShowMessage("DEBUG: Đã mở khóa map tiếp theo!", 3f);
                }
                return;
            }

            // Tải scene tương ứng (nếu không ở chế độ debug)
            if (mapIndex < maps.Count && !string.IsNullOrEmpty(maps[mapIndex].sceneToLoad))
            {
                Debug.Log("Đang tải map " + (mapIndex + 1) + ": " + maps[mapIndex].sceneToLoad);

                // Kiểm tra xem scene có tồn tại trong build settings không
                bool sceneExists = false;
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    if (sceneName.Equals(maps[mapIndex].sceneToLoad, System.StringComparison.OrdinalIgnoreCase))
                    {
                        sceneExists = true;
                        break;
                    }
                }

                if (sceneExists)
                {
                    SceneManager.LoadScene(maps[mapIndex].sceneToLoad);
                }
                else
                {
                    Debug.LogError("Scene '" + maps[mapIndex].sceneToLoad + "' không tồn tại trong Build Settings!");
                    ShowMessage("Lỗi: Scene không tồn tại! Bật Debug Mode và kiểm tra Build Settings.", 5f);
                }
            }
            else
            {
                Debug.LogError("Không tìm thấy scene cho map " + (mapIndex + 1));
                ShowMessage("Lỗi: Không tìm thấy scene cho map này! Hãy bật chế độ Debug.", 5f);
            }
        }
        else
        {
            Debug.Log("Map " + (mapIndex + 1) + " chưa được mở khóa");
            ShowMessage("Map này chưa được mở khóa! Hãy hoàn thành map trước đó.", 3f);
        }
    }

    // Giả lập hoàn thành tất cả nhiệm vụ của map (cho debug mode)
    private void SimulateCompleteMissions(int mapIndex)
    {
        if (mapIndex >= maps.Count) return;

        MapConfig mapConfig = maps[mapIndex];

        // Hoàn thành coins
        if (mapConfig.requiredCoins > 0)
        {
            PlayerPrefs.SetInt(COLLECTED_COINS_KEY + mapIndex, mapConfig.requiredCoins);
        }

        // Hoàn thành chests
        if (mapConfig.requiredChests > 0)
        {
            PlayerPrefs.SetInt(COLLECTED_CHESTS_KEY + mapIndex, mapConfig.requiredChests);
        }

        // Hoàn thành boss
        if (mapConfig.requireDeadKnight)
        {
            PlayerPrefs.SetInt(DEFEATED_DEADKNIGHT_KEY + mapIndex, 1);
        }

        if (mapConfig.requireAshe)
        {
            PlayerPrefs.SetInt(DEFEATED_ASHE_KEY + mapIndex, 1);
        }

        if (mapConfig.requireZombie)
        {
            PlayerPrefs.SetInt(DEFEATED_ZOMBIE_KEY + mapIndex, 1);
        }

        PlayerPrefs.Save();
    }

    private void InitializeMapSelection()
    {
        // Đảm bảo ít nhất map 1 đã được mở khóa
        int unlockedMaps = PlayerPrefs.GetInt(UNLOCKED_MAPS_KEY, 1);
        if (unlockedMaps < 1)
        {
            unlockedMaps = 1;
            PlayerPrefs.SetInt(UNLOCKED_MAPS_KEY, unlockedMaps);
            PlayerPrefs.Save();
        }

        // Thiết lập các nút map
        for (int i = 0; i < mapButtons.Count && i < maps.Count; i++)
        {
            int mapIndex = i; // Cần biến cục bộ để sử dụng trong lambda expression

            Button button = mapButtons[i];
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => LoadMap(mapIndex));
            }
        }

        // Thiết lập nút Trở lại
        Button backButton = null;
        if (mapSelectionPanel != null)
        {
            // Tìm nút Trở lại trong mapSelectionPanel
            Button[] buttons = mapSelectionPanel.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                TextMeshProUGUI tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null && tmpText.text == "Trở lại")
                {
                    backButton = btn;
                    break;
                }
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(HideMapSelection);
                Debug.Log("Đã thiết lập chức năng cho nút Trở lại");
            }
        }
    }

    public void UpdateMapUI()
    {
        int unlockedMaps = PlayerPrefs.GetInt(UNLOCKED_MAPS_KEY, 1);

        // Đảm bảo giá trị hợp lệ
        if (unlockedMaps <= 0 || unlockedMaps > maps.Count)
        {
            unlockedMaps = 1;
            PlayerPrefs.SetInt(UNLOCKED_MAPS_KEY, unlockedMaps);
            PlayerPrefs.Save();
        }

        // Cập nhật trạng thái mở khóa và hiển thị nhiệm vụ cho mỗi map
        for (int i = 0; i < mapButtons.Count && i < maps.Count; i++)
        {
            // Cập nhật trạng thái button
            if (mapButtons[i] != null)
            {
                // Mở/khóa button
                mapButtons[i].interactable = (i < unlockedMaps);

                // Cập nhật độ mờ của button
                CanvasGroup canvasGroup = mapButtons[i].GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = (i < unlockedMaps) ? 1f : 0.5f;
                }
                else
                {
                    // Nếu không có CanvasGroup, điều chỉnh màu
                    ColorBlock colors = mapButtons[i].colors;
                    colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                    mapButtons[i].colors = colors;
                }
            }

            // Cập nhật text hiển thị nhiệm vụ
            if (i < maps.Count)
            {
                MapConfig mapConfig = maps[i];
                int collectedCoins = PlayerPrefs.GetInt(COLLECTED_COINS_KEY + i, 0);
                int collectedChests = PlayerPrefs.GetInt(COLLECTED_CHESTS_KEY + i, 0);

                // Lấy trạng thái boss
                bool deadKnightDefeated = PlayerPrefs.GetInt(DEFEATED_DEADKNIGHT_KEY + i, 0) > 0;
                bool asheDefeated = PlayerPrefs.GetInt(DEFEATED_ASHE_KEY + i, 0) > 0;
                bool zombieDefeated = PlayerPrefs.GetInt(DEFEATED_ZOMBIE_KEY + i, 0) > 0;

                // Cập nhật text xu
                if (i < coinTexts.Count && coinTexts[i] != null)
                {
                    if (mapConfig.requiredCoins > 0)
                    {
                        coinTexts[i].text = collectedCoins + "/" + mapConfig.requiredCoins;
                        coinTexts[i].color = (collectedCoins >= mapConfig.requiredCoins) ? Color.green : Color.red;
                        if (coinTexts[i].transform.parent != null)
                        {
                            coinTexts[i].transform.parent.gameObject.SetActive(true);
                        }
                    }
                    else if (coinTexts[i].transform.parent != null)
                    {
                        coinTexts[i].transform.parent.gameObject.SetActive(false);
                    }
                }

                // Cập nhật text rương
                if (i < chestTexts.Count && chestTexts[i] != null)
                {
                    if (mapConfig.requiredChests > 0)
                    {
                        chestTexts[i].text = collectedChests + "/" + mapConfig.requiredChests;
                        chestTexts[i].color = (collectedChests >= mapConfig.requiredChests) ? Color.green : Color.red;
                        if (chestTexts[i].transform.parent != null)
                        {
                            chestTexts[i].transform.parent.gameObject.SetActive(true);
                        }
                    }
                    else if (chestTexts[i].transform.parent != null)
                    {
                        chestTexts[i].transform.parent.gameObject.SetActive(false);
                    }
                }

                // Cập nhật text boss
                if (i < bossTexts.Count && bossTexts[i] != null)
                {
                    // Đếm số boss đã đánh bại và tổng số yêu cầu
                    int defeatedBossCount = 0;
                    int requiredBossCount = 0;

                    if (mapConfig.requireDeadKnight)
                    {
                        requiredBossCount++;
                        if (deadKnightDefeated) defeatedBossCount++;
                    }

                    if (mapConfig.requireAshe)
                    {
                        requiredBossCount++;
                        if (asheDefeated) defeatedBossCount++;
                    }

                    if (mapConfig.requireZombie)
                    {
                        requiredBossCount++;
                        if (zombieDefeated) defeatedBossCount++;
                    }

                    if (requiredBossCount > 0)
                    {
                        bossTexts[i].text = defeatedBossCount + "/" + requiredBossCount;
                        bossTexts[i].color = (defeatedBossCount >= requiredBossCount) ? Color.green : Color.red;
                        if (bossTexts[i].transform.parent != null)
                        {
                            bossTexts[i].transform.parent.gameObject.SetActive(true);
                        }
                    }
                    else if (bossTexts[i].transform.parent != null)
                    {
                        bossTexts[i].transform.parent.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    // Hiển thị thông báo
    public void ShowMessage(string message, float duration = 2f)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = message;
            messagePanel.SetActive(true);

            // Tự động ẩn sau một khoảng thời gian
            CancelInvoke("HideMessage");
            Invoke("HideMessage", duration);
        }
        else
        {
            Debug.LogWarning("Message panel hoặc text chưa được thiết lập!");
        }
    }

    // Ẩn thông báo
    public void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    // Phương thức này được gọi khi người chơi đến đích hoặc khi kiểm tra hoàn thành
    public static bool CheckMapObjectives(int mapIndex)
    {
        MapSelectionManager instance = FindObjectOfType<MapSelectionManager>();
        if (instance == null || mapIndex >= instance.maps.Count)
        {
            Debug.LogError("Không tìm thấy MapSelectionManager hoặc cấu hình cho map " + (mapIndex + 1));
            return false;
        }

        MapConfig mapConfig = instance.maps[mapIndex];
        int collectedCoins = PlayerPrefs.GetInt(COLLECTED_COINS_KEY + mapIndex, 0);
        int collectedChests = PlayerPrefs.GetInt(COLLECTED_CHESTS_KEY + mapIndex, 0);

        // Kiểm tra các boss
        bool deadKnightComplete = !mapConfig.requireDeadKnight || PlayerPrefs.GetInt(DEFEATED_DEADKNIGHT_KEY + mapIndex, 0) > 0;
        bool asheComplete = !mapConfig.requireAshe || PlayerPrefs.GetInt(DEFEATED_ASHE_KEY + mapIndex, 0) > 0;
        bool zombieComplete = !mapConfig.requireZombie || PlayerPrefs.GetInt(DEFEATED_ZOMBIE_KEY + mapIndex, 0) > 0;

        // Kiểm tra nhiệm vụ
        bool coinsComplete = collectedCoins >= mapConfig.requiredCoins;
        bool chestsComplete = collectedChests >= mapConfig.requiredChests;
        bool bossesComplete = deadKnightComplete && asheComplete && zombieComplete;

        // Nếu hoàn thành tất cả, mở khóa map tiếp theo
        bool allComplete = coinsComplete && chestsComplete && bossesComplete;
        if (allComplete)
        {
            UnlockNextMap(mapIndex);
        }

        return allComplete;
    }

    // Các phương thức static để cập nhật tiến độ khi chơi
    public static void AddCollectedCoin(int mapIndex)
    {
        int currentCoins = PlayerPrefs.GetInt(COLLECTED_COINS_KEY + mapIndex, 0);
        PlayerPrefs.SetInt(COLLECTED_COINS_KEY + mapIndex, currentCoins + 1);
        PlayerPrefs.Save();

        // Kiểm tra hoàn thành map
        CheckMapObjectives(mapIndex);
    }

    public static void AddCollectedChest(int mapIndex)
    {
        int currentChests = PlayerPrefs.GetInt(COLLECTED_CHESTS_KEY + mapIndex, 0);
        PlayerPrefs.SetInt(COLLECTED_CHESTS_KEY + mapIndex, currentChests + 1);
        PlayerPrefs.Save();

        // Kiểm tra hoàn thành map
        CheckMapObjectives(mapIndex);
    }

    // Phương thức để ghi nhận đánh bại boss
    public static void DefeatBoss(string bossType, int mapIndex)
    {
        if (string.IsNullOrEmpty(bossType)) return;

        if (bossType == "DeadKnight")
        {
            PlayerPrefs.SetInt(DEFEATED_DEADKNIGHT_KEY + mapIndex, 1);
            Debug.Log("Đã đánh bại DeadKnight trong map " + (mapIndex + 1));
        }
        else if (bossType == "Ashe")
        {
            PlayerPrefs.SetInt(DEFEATED_ASHE_KEY + mapIndex, 1);
            Debug.Log("Đã đánh bại Ashe trong map " + (mapIndex + 1));
        }
        else if (bossType == "Zombie")
        {
            PlayerPrefs.SetInt(DEFEATED_ZOMBIE_KEY + mapIndex, 1);
            Debug.Log("Đã đánh bại Zombie trong map " + (mapIndex + 1));
        }
        else
        {
            Debug.LogWarning("Loại boss không được hỗ trợ: " + bossType);
            return;
        }

        PlayerPrefs.Save();

        // Kiểm tra hoàn thành map
        CheckMapObjectives(mapIndex);

        // Cập nhật UI nếu có thể
        MapSelectionManager instance = FindObjectOfType<MapSelectionManager>();
        if (instance != null)
        {
            instance.UpdateMapUI();
        }
    }

    // Thêm phương thức này để reset toàn bộ dữ liệu và sửa lỗi
    public void HardResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(UNLOCKED_MAPS_KEY, 1); // Chỉ mở khóa map 1
        PlayerPrefs.SetInt("CurrentMap", 0); // Set map hiện tại về 0 (map đầu tiên)

        // Reset tất cả tiến độ
        for (int i = 0; i < maps.Count; i++)
        {
            PlayerPrefs.SetInt(COLLECTED_COINS_KEY + i, 0);
            PlayerPrefs.SetInt(COLLECTED_CHESTS_KEY + i, 0);
            PlayerPrefs.SetInt(DEFEATED_DEADKNIGHT_KEY + i, 0);
            PlayerPrefs.SetInt(DEFEATED_ASHE_KEY + i, 0);
            PlayerPrefs.SetInt(DEFEATED_ZOMBIE_KEY + i, 0);
        }

        PlayerPrefs.Save();
        UpdateMapUI();
        Debug.Log("Đã reset TOÀN BỘ tiến độ và đặt lại về trạng thái ban đầu!");
        ShowMessage("Đã reset TOÀN BỘ tiến độ!", 3f);
    }

    // Phương thức để mở khóa map tiếp theo
    public static void UnlockNextMap(int currentMapIndex)
    {
        MapSelectionManager instance = FindObjectOfType<MapSelectionManager>();
        if (instance == null)
        {
            Debug.LogError("Không tìm thấy MapSelectionManager!");
            return;
        }

        int unlockedMaps = PlayerPrefs.GetInt(UNLOCKED_MAPS_KEY, 1);
        int nextMapIndex = currentMapIndex + 1;

        // Kiểm tra xem map tiếp theo có tồn tại không
        if (nextMapIndex < instance.maps.Count && nextMapIndex >= unlockedMaps)
        {
            PlayerPrefs.SetInt(UNLOCKED_MAPS_KEY, nextMapIndex + 1); // +1 vì chúng ta đang lưu số map đã mở khóa
            PlayerPrefs.Save();
            Debug.Log("Đã mở khóa map " + (nextMapIndex + 1));

            // Cập nhật UI
            instance.UpdateMapUI();
        }
    }

    // Phương thức để reset tiến độ (cho test)
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(UNLOCKED_MAPS_KEY, 1); // Chỉ mở khóa map 1
        PlayerPrefs.Save();
        UpdateMapUI();
        Debug.Log("Đã reset toàn bộ tiến độ!");
        ShowMessage("Đã reset toàn bộ tiến độ!");
    }

    // Phương thức để mở khóa tất cả map (cho test)
    public void UnlockAllMaps()
    {
        int mapCount = Mathf.Min(maps.Count, 7); // Đảm bảo không vượt quá 7 map
        PlayerPrefs.SetInt(UNLOCKED_MAPS_KEY, mapCount);
        PlayerPrefs.Save();
        UpdateMapUI();
        Debug.Log("Đã mở khóa " + mapCount + " maps!");
        ShowMessage("Đã mở khóa tất cả map!", 3f);
    }

    // Phương thức để in ra tất cả PlayerPrefs
    public void PrintAllPlayerPrefs()
    {
        Debug.Log("===== TẤT CẢ PLAYERPREFS =====");
        Debug.Log("UnlockedMaps: " + PlayerPrefs.GetInt(UNLOCKED_MAPS_KEY, 1));
        Debug.Log("CurrentMap: " + PlayerPrefs.GetInt("CurrentMap", 0));

        for (int i = 0; i < maps.Count; i++)
        {
            Debug.Log("--- Map " + (i + 1) + " ---");
            Debug.Log("Coins: " + PlayerPrefs.GetInt(COLLECTED_COINS_KEY + i, 0));
            Debug.Log("Chests: " + PlayerPrefs.GetInt(COLLECTED_CHESTS_KEY + i, 0));
            Debug.Log("DeadKnight: " + PlayerPrefs.GetInt(DEFEATED_DEADKNIGHT_KEY + i, 0));
            Debug.Log("Ashe: " + PlayerPrefs.GetInt(DEFEATED_ASHE_KEY + i, 0));
            Debug.Log("Zombie: " + PlayerPrefs.GetInt(DEFEATED_ZOMBIE_KEY + i, 0));
        }
    }
}

// Lớp cấu hình cho mỗi map
[System.Serializable]
public class MapConfig
{
    public string mapName;
    public string sceneToLoad;

    [Header("Nhiệm vụ Cơ bản")]
    public int requiredCoins = 0;
    public int requiredChests = 0;

    [Header("Nhiệm vụ Boss")]
    public bool requireDeadKnight = false;
    public bool requireAshe = false;
    public bool requireZombie = false;
}
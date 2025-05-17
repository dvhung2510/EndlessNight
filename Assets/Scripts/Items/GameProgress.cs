using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameProgress : MonoBehaviour
{
    // Hỗ trợ cả hai cách viết để tương thích
    public static GameProgress instance;
    public static GameProgress Instance { get { return instance; } }

    [Header("Map Progress")]
    public bool[] mapUnlocked = new bool[7]; // index 0 = HomeScene, index 1-6 cho Map1-Map6
    public bool[] mapCompleted = new bool[7]; // index 0 = HomeScene, index 1-6 cho Map1-Map6
    public int currentMap = 0; // 0 = HomeScene, 1-6 = Map1-Map6

    [Header("Map Requirements")]
    // Thông số yêu cầu để hoàn thành mỗi map - dựa theo ảnh
    public int[] requiredCoins = { 0, 30, 45, 60, 80, 100, 0 }; // Coin theo từng map
    public int[] collectedCoins = new int[7]; // index 0 = Home, 1-6 = Map1-Map6

    public int[] requiredChests = { 0, 1, 1, 1, 0, 0, 0 }; // 1 rương cho mỗi map
    public int[] collectedChests = new int[7]; // Số rương đã thu thập cho mỗi map

    [Header("Boss Requirements")]
    // Map 1, 2, 3 không yêu cầu DeadKnight; Map 4, 6 có yêu cầu
    public bool[] requireDeadKnight = { false, false, false, false, true, false, true };
    public bool[] defeatedDeadKnight = new bool[7];

    // Map 1, 2, 3, 4, 5 không yêu cầu Ashe; Map 6 có yêu cầu
    public bool[] requireAshe = { false, false, false, false, false, true, true };
    public bool[] defeatedAshe = new bool[7];

    // Map 1, 2, 3 không yêu cầu Zombie; Map 4, 5, 6 có yêu cầu
    public bool[] requireZombie = { false, false, false, false, false, false, true };
    public bool[] defeatedZombie = new bool[7];

    [Header("Debug")]
    public bool resetProgressOnStart = false;
    public bool unlockAllMaps = false;
    // Thêm biến để kiểm soát việc update currentMap từ scene name
    public bool autoUpdateMapFromSceneName = true;

    // Dictionary để lưu trữ tên scene và index tương ứng
    private Dictionary<string, int> sceneNameToIndex = new Dictionary<string, int>() {
        {"HomeScene", 0},
        {"Map1", 1},
        {"Map2", 2},
        {"Map3", 3},
        {"Map4", 4},
        {"Map5", 5},
        {"Map6", 6}
    };

    // Dictionary để lưu trữ index và tên scene tương ứng
    private Dictionary<int, string> indexToSceneName = new Dictionary<int, string>() {
        {0, "HomeScene"},
        {1, "Map1"},
        {2, "Map2"},
        {3, "Map3"},
        {4, "Map4"},
        {5, "Map5"},
        {6, "Map6"}
    };

    // Thêm biến để theo dõi việc đã khởi tạo chưa
    private bool isInitialized = false;

    private void OnEnable()
    {
        Debug.Log("GameProgress OnEnable: Instance ID = " + GetInstanceID());

        // In ra kích thước các mảng
        Debug.Log("Kích thước mảng mapUnlocked = " + mapUnlocked.Length);
        Debug.Log("Kích thước mảng requiredCoins = " + requiredCoins.Length);
    }

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameProgress: Tạo instance mới với ID = " + GetInstanceID());

            // Đảm bảo tất cả các mảng có kích thước 7
            if (mapUnlocked.Length != 7)
            {
                Debug.LogError("Lỗi: Kích thước mảng mapUnlocked không phải 7!");
                mapUnlocked = new bool[7];
            }

            // Tải currentMap đã lưu từ PlayerPrefs
            int savedMap = PlayerPrefs.GetInt("CurrentMap", 0);
            Debug.Log("GameProgress Awake: Đọc CurrentMap từ PlayerPrefs = " + savedMap);
            currentMap = savedMap;

            // Khởi tạo tiến trình
            InitializeProgress();

            // Đánh dấu đã khởi tạo
            isInitialized = true;

            // Đăng ký sự kiện khi scene được load
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Debug.LogWarning("GameProgress: Đã có instance khác (ID = " + instance.GetInstanceID() +
                          "), hủy instance này (ID = " + GetInstanceID() + ")");
            Destroy(gameObject);
        }
    }

    // Phương thức được gọi khi scene được load
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("GameProgress OnSceneLoaded: " + scene.name);
        // Cập nhật currentMap dựa trên tên scene nếu được phép
        if (autoUpdateMapFromSceneName)
        {
            UpdateCurrentMapFromSceneName();
        }
        else
        {
            Debug.Log("Bỏ qua việc cập nhật currentMap từ tên scene (autoUpdateMapFromSceneName = false)");
        }
    }

    // Phương thức cập nhật currentMap dựa trên tên scene hiện tại
    public void UpdateCurrentMapFromSceneName()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("UpdateCurrentMapFromSceneName: Scene hiện tại = " + currentSceneName);

        // Vấn đề ở đây có thể là cách xử lý tên scene
        int mapIndex = -1;

        // Kiểm tra trực tiếp tên scene
        if (currentSceneName == "HomeScene" || currentSceneName.Contains("Home"))
        {
            mapIndex = 0;
            Debug.Log("Scene hiện tại là HomeScene, mapIndex = 0");
        }
        else if (currentSceneName == "Map1" || currentSceneName.Contains("Map1"))
        {
            mapIndex = 1;
            Debug.Log("Scene hiện tại là Map1, mapIndex = 1");
        }
        else if (currentSceneName == "Map2" || currentSceneName.Contains("Map2"))
        {
            mapIndex = 2;
        }
        else if (currentSceneName == "Map3" || currentSceneName.Contains("Map3"))
        {
            mapIndex = 3;
        }
        else if (currentSceneName == "Map4" || currentSceneName.Contains("Map4"))
        {
            mapIndex = 4;
        }
        else if (currentSceneName == "Map5" || currentSceneName.Contains("Map5"))
        {
            mapIndex = 5;
        }
        else if (currentSceneName == "Map6" || currentSceneName.Contains("Map6"))
        {
            mapIndex = 6;
        }
        else
        {
            // Tên scene không khớp với bất kỳ map nào
            Debug.LogWarning("Không thể xác định map index từ scene name: " + currentSceneName);
            return; // Không cập nhật currentMap
        }

        // Chỉ cập nhật currentMap nếu tìm thấy mapIndex hợp lệ
        if (mapIndex >= 0)
        {
            // Lưu giá trị cũ để in log
            int oldMap = currentMap;
            currentMap = mapIndex;

            Debug.Log("UpdateCurrentMapFromSceneName: Đã cập nhật currentMap từ " + oldMap +
                   " thành " + currentMap + " dựa trên tên scene " + currentSceneName);

            // Lưu giá trị currentMap
            PlayerPrefs.SetInt("CurrentMap", currentMap);
            PlayerPrefs.Save();
        }
    }

    private void InitializeProgress()
    {
        if (resetProgressOnStart)
        {
            ResetProgress();
        }
        else
        {
            LoadProgress();
        }

        // Map 1 luôn được mở khóa mặc định
        mapUnlocked[1] = true;

        // Home cũng luôn được mở khóa
        mapUnlocked[0] = true;

        // Nếu debug mode - mở khóa tất cả map
        if (unlockAllMaps)
        {
            for (int i = 0; i < mapUnlocked.Length; i++)
            {
                mapUnlocked[i] = true;
            }
        }

        SaveProgress();
    }

    // Đặt map hiện tại
    public void SetCurrentMap(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex <= 6)
        {
            int oldMap = currentMap;
            currentMap = mapIndex;
            Debug.Log("Đã cập nhật currentMap từ " + oldMap + " thành " + mapIndex + " thông qua SetCurrentMap");

            PlayerPrefs.SetInt("CurrentMap", currentMap);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("Không thể thiết lập currentMap = " + mapIndex + ": Giá trị không hợp lệ (phải từ 0-6)");
        }
    }

    // Phương thức để lấy tên scene từ index map
    public string GetSceneNameFromMapIndex(int mapIndex)
    {
        if (indexToSceneName.TryGetValue(mapIndex, out string sceneName))
        {
            return sceneName;
        }
        Debug.LogError("Không tìm thấy tên scene cho mapIndex: " + mapIndex);
        return "HomeScene"; // Mặc định về Home nếu không tìm thấy
    }

    // Phương thức để chuyển đến map tiếp theo
    public void GoToNextMap()
    {
        int nextMapIndex = currentMap + 1;
        if (nextMapIndex < mapUnlocked.Length && mapUnlocked[nextMapIndex])
        {
            // Tạm thời tắt cập nhật tự động từ tên scene để tránh xung đột
            autoUpdateMapFromSceneName = false;

            // Lưu map mới
            SetCurrentMap(nextMapIndex);

            // Lấy tên scene từ index map và load scene
            string nextSceneName = GetSceneNameFromMapIndex(nextMapIndex);
            Debug.Log("Chuyển đến map tiếp theo: " + nextSceneName + " (index: " + nextMapIndex + ")");
            SceneManager.LoadScene(nextSceneName);

            // Bật lại cập nhật tự động sau một frame
            StartCoroutine(ReEnableAutoUpdate());
        }
        else
        {
            Debug.LogWarning("Không thể chuyển đến map tiếp theo: Map " + nextMapIndex + " chưa được mở khóa.");
        }
    }

    // Coroutine để bật lại cập nhật tự động
    private System.Collections.IEnumerator ReEnableAutoUpdate()
    {
        yield return null; // Đợi 1 frame
        autoUpdateMapFromSceneName = true;
        Debug.Log("Đã bật lại autoUpdateMapFromSceneName");
    }

    // Kiểm tra xem map có được mở khóa chưa
    public bool IsMapUnlocked(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < mapUnlocked.Length)
        {
            return mapUnlocked[mapIndex];
        }
        return false;
    }

    // Kiểm tra xem map đã hoàn thành chưa
    public bool IsMapCompleted(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < mapCompleted.Length)
        {
            return mapCompleted[mapIndex];
        }
        return false;
    }

    // Kiểm tra xem game đã hoàn thành chưa (tất cả các map đã hoàn thành)
    public bool IsGameCompleted()
    {
        for (int i = 1; i < mapCompleted.Length; i++)
        {
            if (!mapCompleted[i])
                return false;
        }
        return true;
    }

    // Mở khóa map mới
    public void UnlockMap(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < mapUnlocked.Length)
        {
            mapUnlocked[mapIndex] = true;
            SaveProgress();
            Debug.Log("Đã mở khóa Map " + mapIndex);
        }
    }

    // Đánh dấu map đã hoàn thành
    public void CompleteMap(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < mapCompleted.Length)
        {
            mapCompleted[mapIndex] = true;

            // Mở khóa map tiếp theo (nếu không phải map cuối)
            if (mapIndex < 6)
            {
                UnlockMap(mapIndex + 1);
            }

            SaveProgress();
            Debug.Log("Đã hoàn thành Map " + mapIndex);
        }
    }

    // Thu thập coin
    public void CollectCoin(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < collectedCoins.Length)
        {
            collectedCoins[mapIndex]++;
            SaveProgress();
            Debug.Log("Đã thu thập Coin trong Map " + mapIndex + " (Tổng: " + collectedCoins[mapIndex] + "/" + requiredCoins[mapIndex] + ")");
        }
    }

    // Phương thức thay thế cho CollectCoin để tương thích
    public void AddCoin()
    {
        // Sử dụng currentMap thay vì cần truyền mapIndex
        CollectCoin(currentMap);
    }

    // Phương thức thay thế cho CollectCoin với tham số để tương thích
    public void AddCoin(int mapIndex)
    {
        CollectCoin(mapIndex);
    }

    // Thu thập rương
    public void CollectChest(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < collectedChests.Length)
        {
            collectedChests[mapIndex]++;
            SaveProgress();
            Debug.Log("Đã thu thập Rương trong Map " + mapIndex + " (Tổng: " + collectedChests[mapIndex] + "/" + requiredChests[mapIndex] + ")");
        }
    }

    // Phương thức thay thế cho CollectChest không cần tham số
    public void AddChest()
    {
        // Sử dụng currentMap thay vì cần truyền mapIndex
        CollectChest(currentMap);
    }

    // Phương thức thay thế cho CollectChest để tương thích
    public void AddChest(int mapIndex)
    {
        CollectChest(mapIndex);
    }

    // Các phương thức tương thích cho HealthPotionSystem - giữ nguyên

    // Đánh bại boss - phiên bản không truyền mapIndex
    public void DefeatBoss(string bossType)
    {
        // Sử dụng currentMap thay vì cần truyền mapIndex
        DefeatBoss(bossType, currentMap);
    }

    // Đánh bại boss - phiên bản có truyền mapIndex
    public void DefeatBoss(string bossType, int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= defeatedDeadKnight.Length)
        {
            Debug.LogError("DefeatBoss: mapIndex không hợp lệ: " + mapIndex);
            return;
        }

        switch (bossType.ToLower())
        {
            case "deadknight":
                defeatedDeadKnight[mapIndex] = true;
                Debug.Log("Đã đánh bại DeadKnight trong Map " + mapIndex);
                break;
            case "ashe":
                defeatedAshe[mapIndex] = true;
                Debug.Log("Đã đánh bại Ashe trong Map " + mapIndex);
                break;
            case "zombie":
                defeatedZombie[mapIndex] = true;
                Debug.Log("Đã đánh bại Zombie trong Map " + mapIndex);
                break;
            default:
                Debug.LogWarning("Loại boss không được hỗ trợ: " + bossType);
                return;
        }

        SaveProgress();

        // Kiểm tra xem map đã hoàn thành chưa
        if (HasEnoughItems(mapIndex))
        {
            Debug.Log("Sau khi đánh bại boss, đã đủ điều kiện hoàn thành map " + mapIndex);
            // Có thể thêm xử lý khác ở đây nếu cần
        }
    }

    // Kiểm tra xem có đủ điều kiện hoàn thành map không - phiên bản không truyền mapIndex
    public bool HasEnoughItems()
    {
        // Sử dụng currentMap
        return HasEnoughItems(currentMap);
    }

    // Kiểm tra xem có đủ điều kiện hoàn thành map không
    public bool HasEnoughItems(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= requiredCoins.Length)
        {
            Debug.LogError("HasEnoughItems: MapIndex không hợp lệ: " + mapIndex);
            return true; // Trả về true để tránh lỗi
        }

        // Kiểm tra số coin
        bool enoughCoins = collectedCoins[mapIndex] >= requiredCoins[mapIndex];
        Debug.Log("Đủ xu? " + enoughCoins + " (" + collectedCoins[mapIndex] +
                  "/" + requiredCoins[mapIndex] + ")");

        // Kiểm tra số rương
        bool enoughChests = collectedChests[mapIndex] >= requiredChests[mapIndex];
        Debug.Log("Đủ rương? " + enoughChests + " (" + collectedChests[mapIndex] +
                  "/" + requiredChests[mapIndex] + ")");

        // Kiểm tra đã đánh bại các boss được yêu cầu chưa
        bool defeatedRequiredBosses = true;

        if (requireDeadKnight[mapIndex] && !defeatedDeadKnight[mapIndex])
        {
            defeatedRequiredBosses = false;
            Debug.Log("Chưa đánh bại DeadKnight");
        }

        if (requireAshe[mapIndex] && !defeatedAshe[mapIndex])
        {
            defeatedRequiredBosses = false;
            Debug.Log("Chưa đánh bại Ashe");
        }

        if (requireZombie[mapIndex] && !defeatedZombie[mapIndex])
        {
            defeatedRequiredBosses = false;
            Debug.Log("Chưa đánh bại Zombie");
        }

        Debug.Log("Đã đánh bại tất cả boss yêu cầu? " + defeatedRequiredBosses);

        bool result = enoughCoins && enoughChests && defeatedRequiredBosses;
        Debug.Log("Kết quả kiểm tra nhiệm vụ: " + result);

        return result;
    }

    // Lưu tiến trình
    public void SaveProgress()
    {
        // Lưu current map
        PlayerPrefs.SetInt("CurrentMap", currentMap);

        // Lưu trạng thái mở khóa của các map
        for (int i = 0; i < mapUnlocked.Length; i++)
        {
            PlayerPrefs.SetInt("MapUnlocked_" + i, mapUnlocked[i] ? 1 : 0);
        }

        // Lưu trạng thái hoàn thành của các map
        for (int i = 0; i < mapCompleted.Length; i++)
        {
            PlayerPrefs.SetInt("MapCompleted_" + i, mapCompleted[i] ? 1 : 0);
        }

        // Lưu số coin đã thu thập
        for (int i = 0; i < collectedCoins.Length; i++)
        {
            PlayerPrefs.SetInt("CollectedCoins_" + i, collectedCoins[i]);
        }

        // Lưu số rương đã thu thập
        for (int i = 0; i < collectedChests.Length; i++)
        {
            PlayerPrefs.SetInt("CollectedChests_" + i, collectedChests[i]);
        }

        // Lưu trạng thái boss
        for (int i = 0; i < defeatedDeadKnight.Length; i++)
        {
            PlayerPrefs.SetInt("DefeatedDeadKnight_" + i, defeatedDeadKnight[i] ? 1 : 0);
            PlayerPrefs.SetInt("DefeatedAshe_" + i, defeatedAshe[i] ? 1 : 0);
            PlayerPrefs.SetInt("DefeatedZombie_" + i, defeatedZombie[i] ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    // Tải tiến trình
    public void LoadProgress()
    {
        // Tải currentMap
        currentMap = PlayerPrefs.GetInt("CurrentMap", 0);
        Debug.Log("LoadProgress: Đã tải currentMap = " + currentMap);

        // Tải trạng thái mở khóa của các map
        for (int i = 0; i < mapUnlocked.Length; i++)
        {
            // Home luôn được mở khóa, Map1 mặc định được mở khóa
            int defaultValue = (i == 0 || i == 1) ? 1 : 0;
            mapUnlocked[i] = PlayerPrefs.GetInt("MapUnlocked_" + i, defaultValue) == 1;
        }

        // Tải trạng thái hoàn thành của các map
        for (int i = 0; i < mapCompleted.Length; i++)
        {
            mapCompleted[i] = PlayerPrefs.GetInt("MapCompleted_" + i, 0) == 1;
        }

        // Tải số coin đã thu thập
        for (int i = 0; i < collectedCoins.Length; i++)
        {
            collectedCoins[i] = PlayerPrefs.GetInt("CollectedCoins_" + i, 0);
        }

        // Tải số rương đã thu thập
        for (int i = 0; i < collectedChests.Length; i++)
        {
            collectedChests[i] = PlayerPrefs.GetInt("CollectedChests_" + i, 0);
        }

        // Tải trạng thái boss
        for (int i = 0; i < defeatedDeadKnight.Length; i++)
        {
            defeatedDeadKnight[i] = PlayerPrefs.GetInt("DefeatedDeadKnight_" + i, 0) == 1;
            defeatedAshe[i] = PlayerPrefs.GetInt("DefeatedAshe_" + i, 0) == 1;
            defeatedZombie[i] = PlayerPrefs.GetInt("DefeatedZombie_" + i, 0) == 1;
        }
    }

    // Reset tất cả tiến trình
    public void ResetProgress()
    {
        // Reset trạng thái mở khóa
        for (int i = 0; i < mapUnlocked.Length; i++)
        {
            mapUnlocked[i] = (i == 0 || i == 1); // Home và Map 1 được mở khóa mặc định
        }

        // Reset trạng thái hoàn thành
        for (int i = 0; i < mapCompleted.Length; i++)
        {
            mapCompleted[i] = false;
        }

        // Reset coin
        for (int i = 0; i < collectedCoins.Length; i++)
        {
            collectedCoins[i] = 0;
        }

        // Reset rương
        for (int i = 0; i < collectedChests.Length; i++)
        {
            collectedChests[i] = 0;
        }

        // Reset boss
        for (int i = 0; i < defeatedDeadKnight.Length; i++)
        {
            defeatedDeadKnight[i] = false;
            defeatedAshe[i] = false;
            defeatedZombie[i] = false;
        }

        SaveProgress();
        Debug.Log("Đã reset tất cả tiến trình!");
    }

    // Phương thức thay thế cho ResetProgress để tương thích (giữ lại cho code cũ)
    public void ResetAllProgress()
    {
        ResetProgress();
    }

    // Thêm phương thức để in thông tin chi tiết
    public void PrintDebugInfo()
    {
        Debug.Log("===== DEBUG INFO - GAME PROGRESS =====");
        Debug.Log("Instance ID: " + GetInstanceID());
        Debug.Log("Is Initialized: " + isInitialized);
        Debug.Log("Current Map: " + currentMap);
        Debug.Log("Map Unlocked Count: " + mapUnlocked.Length);

        for (int i = 0; i < mapUnlocked.Length; i++)
        {
            Debug.Log("Map " + i + ": " +
                    (mapUnlocked[i] ? "Unlocked" : "Locked") + ", " +
                    (mapCompleted[i] ? "Completed" : "Not Completed"));

            if (i > 0) // Map 0 không có yêu cầu
            {
                Debug.Log(" - Coins: " + collectedCoins[i] + "/" + requiredCoins[i]);
                Debug.Log(" - Chests: " + collectedChests[i] + "/" + requiredChests[i]);

                if (requireDeadKnight[i])
                    Debug.Log(" - DeadKnight: " + (defeatedDeadKnight[i] ? "Defeated" : "Not Defeated"));

                if (requireAshe[i])
                    Debug.Log(" - Ashe: " + (defeatedAshe[i] ? "Defeated" : "Not Defeated"));

                if (requireZombie[i])
                    Debug.Log(" - Zombie: " + (defeatedZombie[i] ? "Defeated" : "Not Defeated"));
            }
        }

        Debug.Log("=====================================");
    }

    // Phương thức để force đặt currentMap cho Map1
    public void ForceSetCurrentMapToOne()
    {
        autoUpdateMapFromSceneName = false; // Tạm thời tắt cập nhật tự động
        currentMap = 1;
        PlayerPrefs.SetInt("CurrentMap", 1);
        PlayerPrefs.Save();
        Debug.Log("Đã FORCE đặt currentMap = 1");
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi GameObject bị hủy
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
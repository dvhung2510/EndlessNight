using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using static GameManager;

[System.Serializable]
public class MapRequirement
{
    public int coinsRequired = 0;
    public int chestsRequired = 0;
    public Dictionary<string, int> bossesRequired = new Dictionary<string, int>();
}

[System.Serializable]
public class MapProgress
{
    public int coinsCollected = 0;
    public int chestsCollected = 0;
    public Dictionary<string, int> bossesDefeated = new Dictionary<string, int>();
    public bool isCompleted = false;

    // Hàm chuyển đổi Dictionary thành SerializableDictionary để có thể lưu với JsonUtility
    public SerializableDictionary<string, int> GetSerializableBossDict()
    {
        return SerializableDictionary<string, int>.FromDictionary(bossesDefeated);
    }

    // Hàm để nạp Dictionary từ SerializableDictionary
    public void LoadBossesFromSerializable(SerializableDictionary<string, int> serDict)
    {
        bossesDefeated = serDict.ToDictionary();
    }
}

[System.Serializable]
public class PlayerItems
{
    public int hearts = 0;
    public int stars = 0;
    public int keys = 0;
    public int vipKeys = 0;
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] mapButtons;
    [SerializeField] private int currentMapIndex = 0;

    private MapRequirement[] mapRequirements;
    private MapProgress[] mapProgress;
    private PlayerItems playerItems;

    private const string PROGRESS_KEY = "GameProgress";
    private const string ITEMS_KEY = "PlayerItems";

    private void Awake()
    {
        InitializeMapRequirements();
        LoadGameData();
        UpdateMapSelectionUI();
    }

    private void InitializeMapRequirements()
    {
        // Khởi tạo yêu cầu cho từng map
        mapRequirements = new MapRequirement[6];

        // Map 1: 30 coins và 1 chest
        mapRequirements[0] = new MapRequirement { coinsRequired = 30, chestsRequired = 1 };

        // Map 2: 45 coins và 1 chest
        mapRequirements[1] = new MapRequirement { coinsRequired = 45, chestsRequired = 1 };

        // Map 3: 60 coins và 1 chest
        mapRequirements[2] = new MapRequirement { coinsRequired = 60, chestsRequired = 1 };

        // Map 4: 30 coins và 1 boss loại 1
        mapRequirements[3] = new MapRequirement { coinsRequired = 30, chestsRequired = 0 };
        mapRequirements[3].bossesRequired.Add("BossType1", 1);

        // Map 5: 30 coins và 1 boss loại 1
        mapRequirements[4] = new MapRequirement { coinsRequired = 30, chestsRequired = 0 };
        mapRequirements[4].bossesRequired.Add("BossType1", 1);

        // Map 6: 60 coins, 1 boss loại 1, 1 boss loại 2, 1 boss loại 3
        mapRequirements[5] = new MapRequirement { coinsRequired = 0, chestsRequired = 0 };
        mapRequirements[5].bossesRequired.Add("BossType1", 1);
        mapRequirements[5].bossesRequired.Add("BossType2", 1);
        mapRequirements[5].bossesRequired.Add("BossType3", 1);
    }

    private void LoadGameData()
    {
        // Tải dữ liệu tiến độ map từ PlayerPrefs
        string progressJson = PlayerPrefs.GetString(PROGRESS_KEY, "");
        if (string.IsNullOrEmpty(progressJson))
        {
            // Khởi tạo mảng tiến độ map mới nếu chưa có dữ liệu
            mapProgress = new MapProgress[6];
            for (int i = 0; i < 6; i++)
            {
                mapProgress[i] = new MapProgress();
                if (mapProgress[i].bossesDefeated == null)
                    mapProgress[i].bossesDefeated = new Dictionary<string, int>();
            }
        }
        else
        {
            // Unity JsonUtility không hỗ trợ serialize mảng trực tiếp
            // Cần bọc trong một class trước
            SerializableMapProgress wrapper = JsonUtility.FromJson<SerializableMapProgress>(progressJson);
            mapProgress = wrapper.progress;

            // Đảm bảo Dictionary được khởi tạo cho mỗi MapProgress
            foreach (var progress in mapProgress)
            {
                if (progress.bossesDefeated == null)
                    progress.bossesDefeated = new Dictionary<string, int>();
            }
        }

        // Tải dữ liệu vật phẩm người chơi từ PlayerPrefs
        string itemsJson = PlayerPrefs.GetString(ITEMS_KEY, "");
        if (string.IsNullOrEmpty(itemsJson))
        {
            // Khởi tạo đối tượng vật phẩm mới nếu chưa có dữ liệu
            playerItems = new PlayerItems();
        }
        else
        {
            // Chuyển đổi JSON thành đối tượng vật phẩm
            playerItems = JsonUtility.FromJson<PlayerItems>(itemsJson);
        }
    }

    [System.Serializable]
    public class SerializableMapProgress
    {
        public MapProgress[] progress;
    }

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        public List<TKey> keys = new List<TKey>();
        public List<TValue> values = new List<TValue>();

        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }

        public static SerializableDictionary<TKey, TValue> FromDictionary(Dictionary<TKey, TValue> dict)
        {
            SerializableDictionary<TKey, TValue> serDict = new SerializableDictionary<TKey, TValue>();
            foreach (var kvp in dict)
            {
                serDict.keys.Add(kvp.Key);
                serDict.values.Add(kvp.Value);
            }
            return serDict;
        }
    }

    private void SaveGameData()
    {
        // Vì JsonUtility không hỗ trợ trực tiếp serialize mảng
        // Cần bọc trong một class
        SerializableMapProgress wrapper = new SerializableMapProgress
        {
            progress = mapProgress
        };

        // Lưu dữ liệu tiến độ map vào PlayerPrefs
        string progressJson = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PROGRESS_KEY, progressJson);

        // Lưu dữ liệu vật phẩm người chơi vào PlayerPrefs
        string itemsJson = JsonUtility.ToJson(playerItems);
        PlayerPrefs.SetString(ITEMS_KEY, itemsJson);

        PlayerPrefs.Save();
    }

    private void UpdateMapSelectionUI()
    {
        // Cập nhật giao diện chọn map
        for (int i = 0; i < mapButtons.Length; i++)
        {
            bool isUnlocked = (i == 0) || mapProgress[i - 1].isCompleted;
            mapButtons[i].GetComponent<Button>().interactable = isUnlocked;

            // Cập nhật UI hiển thị tiến độ
            UpdateMapProgressUI(i);
        }
    }

    private void UpdateMapProgressUI(int mapIndex)
    {
        // Phần này sẽ cập nhật hiển thị tiến độ trên UI
        // Ví dụ: Coins 15/30, Chests 0/1, v.v.
        // Cần thêm code cho việc này tùy thuộc vào UI cụ thể của bạn
    }

    public void SelectMap(int mapIndex)
    {
        // Kiểm tra xem map có mở khóa chưa
        if (mapIndex == 0 || mapProgress[mapIndex - 1].isCompleted)
        {
            currentMapIndex = mapIndex;
            LoadSelectedMap();
        }
    }

    private void LoadSelectedMap()
    {
        // Tải scene của map đã chọn
        // Có thể sử dụng SceneManager.LoadScene hoặc phương thức khác tùy thuộc vào cách bạn thiết kế
        Debug.Log("Đang tải map " + (currentMapIndex + 1));
    }

    public void CollectItem(string itemType, int amount)
    {
        // Cập nhật dữ liệu thu thập trong map hiện tại
        switch (itemType)
        {
            case "Coin":
                mapProgress[currentMapIndex].coinsCollected += amount;
                break;
            case "Chest":
                mapProgress[currentMapIndex].chestsCollected += amount;
                break;
            case "Boss":
                string bossType = "BossType1"; // Giả sử đây là loại boss, cần truyền loại boss thực tế
                if (!mapProgress[currentMapIndex].bossesDefeated.ContainsKey(bossType))
                {
                    mapProgress[currentMapIndex].bossesDefeated[bossType] = 0;
                }
                mapProgress[currentMapIndex].bossesDefeated[bossType] += amount;
                break;
            case "Heart":
                playerItems.hearts += amount;
                break;
            case "Star":
                playerItems.stars += amount;
                break;
            case "Key":
                playerItems.keys += amount;
                break;
            case "KeyVIP":
                playerItems.vipKeys += amount;
                break;
        }

        // Kiểm tra xem map hiện tại đã hoàn thành chưa
        CheckMapCompletion();

        // Lưu dữ liệu
        SaveGameData();

        // Cập nhật UI
        UpdateMapProgressUI(currentMapIndex);
    }

    private void CheckMapCompletion()
    {
        MapRequirement req = mapRequirements[currentMapIndex];
        MapProgress prog = mapProgress[currentMapIndex];

        // Kiểm tra coins và chests
        bool coinsCompleted = prog.coinsCollected >= req.coinsRequired;
        bool chestsCompleted = prog.chestsCollected >= req.chestsRequired;

        // Kiểm tra bosses
        bool bossesCompleted = true;
        foreach (var bossReq in req.bossesRequired)
        {
            int defeatedCount = 0;
            prog.bossesDefeated.TryGetValue(bossReq.Key, out defeatedCount);

            if (defeatedCount < bossReq.Value)
            {
                bossesCompleted = false;
                break;
            }
        }

        // Cập nhật trạng thái hoàn thành
        prog.isCompleted = coinsCompleted && chestsCompleted && bossesCompleted;

        // Nếu map hiện tại hoàn thành, cập nhật UI để mở khóa map tiếp theo
        if (prog.isCompleted && currentMapIndex < mapButtons.Length - 1)
        {
            UpdateMapSelectionUI();
        }
    }

    // Hàm này có thể được gọi khi người chơi hoàn thành một map
    public void CompleteCurrentMap()
    {
        mapProgress[currentMapIndex].isCompleted = true;
        SaveGameData();
        UpdateMapSelectionUI();
    }

    // Lấy thông tin vật phẩm của người chơi
    public PlayerItems GetPlayerItems()
    {
        return playerItems;
    }
}

// UI Manager để hiển thị thông tin trên màn hình Map Selection
public class MapSelectionUIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMPro.TextMeshProUGUI playerItemsText;

    private void Start()
    {
        UpdatePlayerItemsUI();
    }

    public void UpdatePlayerItemsUI()
    {
        PlayerItems items = gameManager.GetPlayerItems();
        playerItemsText.text = string.Format(
            "Hearts: {0}\nStars: {1}\nKeys: {2}\nVIP Keys: {3}",
            items.hearts, items.stars, items.keys, items.vipKeys
        );
    }

    // Hàm này sẽ được gọi khi người chơi nhấn vào một map
    public void OnMapButtonClicked(int mapIndex)
    {
        gameManager.SelectMap(mapIndex);
    }
}
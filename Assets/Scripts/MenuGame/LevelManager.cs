using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [System.Serializable]
    public class MapButton
    {
        public Button button;
        public int mapIndex;
        public TextMeshProUGUI coinText;
        // Boss Icons
        public GameObject deadKnightIcon;
        public GameObject asheIcon;
        public GameObject zombieIcon;
        public Image lockIcon;
        public Image completedIcon;
    }

    public MapButton[] mapButtons;
    public Button backButton;

    // Reference đến GameProgress
    private GameProgress gameProgress;

    private void Awake()
    {
        // Thiết lập singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log("LevelManager Start()");

        // Tìm GameProgress
        gameProgress = GameProgress.instance;
        if (gameProgress == null)
        {
            Debug.LogError("Không tìm thấy GameProgress! Cần có GameProgress để hoạt động đúng.");
            // Tạo mới GameProgress nếu cần
            GameObject progressObj = new GameObject("GameProgress");
            gameProgress = progressObj.AddComponent<GameProgress>();
            DontDestroyOnLoad(progressObj);
            Debug.Log("Đã tạo GameProgress mới");
        }

        // Thiết lập UI
        SetupUI();
    }

    // Tách riêng việc thiết lập UI để có thể gọi lại khi cần
    public void SetupUI()
    {
        // Thiết lập nút Back
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => {
                Debug.Log("Nhấn nút Back, chuyển về MainMenu");
                SceneManager.LoadScene("Scenes/MenuGame");
            });
        }

        // Thiết lập các nút map
        if (mapButtons != null && mapButtons.Length > 0)
        {
            foreach (MapButton mapBtn in mapButtons)
            {
                if (mapBtn.button == null) continue;

                int mapIndex = mapBtn.mapIndex;
                Debug.Log("Thiết lập button cho Map " + mapIndex);

                // Cập nhật trạng thái khóa
                bool isUnlocked = gameProgress != null && gameProgress.IsMapUnlocked(mapIndex);
                Debug.Log("Map " + mapIndex + " " + (isUnlocked ? "đã mở khóa" : "đang bị khóa"));

                // Cập nhật trạng thái hoàn thành
                bool isCompleted = gameProgress != null && gameProgress.IsMapCompleted(mapIndex);
                Debug.Log("Map " + mapIndex + " " + (isCompleted ? "đã hoàn thành" : "chưa hoàn thành"));

                // Hiển thị icon khóa nếu map chưa mở
                if (mapBtn.lockIcon != null)
                {
                    mapBtn.lockIcon.gameObject.SetActive(!isUnlocked);
                }

                // Hiển thị icon hoàn thành nếu có
                if (mapBtn.completedIcon != null)
                {
                    mapBtn.completedIcon.gameObject.SetActive(isCompleted);
                }

                // Cập nhật thông tin tiến trình
                UpdateMapButtonInfo(mapBtn);

                // Thiết lập interactable dựa vào trạng thái mở khóa
                mapBtn.button.interactable = isUnlocked;

                // Xóa listener cũ để tránh trùng lặp
                mapBtn.button.onClick.RemoveAllListeners();

                // Thêm event listener cho nút
                int capturedMapIndex = mapIndex; // Tránh vấn đề closure trong lambda
                mapBtn.button.onClick.AddListener(() => {
                    Debug.Log("Nhấn vào Map " + capturedMapIndex);
                    LoadMap(capturedMapIndex);
                });
            }
        }
        else
        {
            Debug.LogWarning("Không có Map Buttons nào được thiết lập!");
        }
    }

    // Cập nhật thông tin hiển thị cho button map
    private void UpdateMapButtonInfo(MapButton mapBtn)
    {
        if (gameProgress != null)
        {
            int mapArrayIndex = mapBtn.mapIndex - 1;

            // Kiểm tra index hợp lệ
            if (mapArrayIndex < 0 || mapArrayIndex >= 6) return;

            // Cập nhật Text hiển thị coin
            if (mapBtn.coinText != null)
            {
                mapBtn.coinText.text = gameProgress.collectedCoins[mapArrayIndex] + "/" +
                                       gameProgress.requiredCoins[mapArrayIndex];
            }

            // Hiển thị/ẩn boss icons dựa vào yêu cầu
            if (mapBtn.deadKnightIcon != null)
            {
                bool required = gameProgress.requireDeadKnight[mapArrayIndex];
                bool defeated = gameProgress.defeatedDeadKnight[mapArrayIndex];

                mapBtn.deadKnightIcon.SetActive(required);

                // Nếu icon có component Image để đổi màu khi đã đánh bại
                Image icon = mapBtn.deadKnightIcon.GetComponent<Image>();
                if (icon != null && required)
                {
                    // Đổi màu icon khi đã đánh bại (ví dụ: xám -> sáng)
                    icon.color = defeated ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                }
            }

            if (mapBtn.asheIcon != null)
            {
                bool required = gameProgress.requireAshe[mapArrayIndex];
                bool defeated = gameProgress.defeatedAshe[mapArrayIndex];

                mapBtn.asheIcon.SetActive(required);

                Image icon = mapBtn.asheIcon.GetComponent<Image>();
                if (icon != null && required)
                {
                    icon.color = defeated ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                }
            }

            if (mapBtn.zombieIcon != null)
            {
                bool required = gameProgress.requireZombie[mapArrayIndex];
                bool defeated = gameProgress.defeatedZombie[mapArrayIndex];

                mapBtn.zombieIcon.SetActive(required);

                Image icon = mapBtn.zombieIcon.GetComponent<Image>();
                if (icon != null && required)
                {
                    icon.color = defeated ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                }
            }
        }
        else
        {
            // Hiển thị giá trị mặc định nếu không tìm thấy GameProgress
            if (mapBtn.coinText != null) mapBtn.coinText.text = "0/30";

            // Ẩn tất cả các boss icons
            if (mapBtn.deadKnightIcon != null) mapBtn.deadKnightIcon.SetActive(false);
            if (mapBtn.asheIcon != null) mapBtn.asheIcon.SetActive(false);
            if (mapBtn.zombieIcon != null) mapBtn.zombieIcon.SetActive(false);
        }
    }

    // Tải map đã chọn - cần phải là PUBLIC để có thể gọi từ Button
    public void LoadMap(int mapIndex)
    {
        Debug.Log("LoadMap được gọi với mapIndex = " + mapIndex);

        // Kiểm tra xem map đã được mở khóa chưa
        if (gameProgress != null && gameProgress.IsMapUnlocked(mapIndex))
        {
            // CẬP NHẬT CURRENT MAP TRONG GAMEPROGRESS
            gameProgress.SetCurrentMap(mapIndex);
            gameProgress.SaveProgress();
            Debug.Log("Đã cập nhật currentMap trong GameProgress: " + mapIndex);

            // Tải scene
            string sceneName = "Scenes/Map" + mapIndex;
            Debug.Log("Đang tải scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Map " + mapIndex + " chưa được mở khóa!");
            // Có thể hiển thị thông báo cho người chơi
        }
    }

    // Mở khóa map tiếp theo - THÊM LẠI HÀM NÀY ĐỂ TƯƠNG THÍCH VỚI GAMEMANGER
    public void UnlockNextMap()
    {
        if (gameProgress == null) return;

        // Lấy index map hiện tại
        int currentMapIndex = gameProgress.currentMap;

        // Mở khóa map tiếp theo nếu chưa phải map cuối
        if (currentMapIndex > 0 && currentMapIndex < 6)
        {
            int nextMapIndex = currentMapIndex + 1;

            // Mở khóa map tiếp theo thông qua GameProgress
            gameProgress.UnlockMap(nextMapIndex);
            Debug.Log("Đã mở khóa Map " + nextMapIndex + " từ LevelManager.UnlockNextMap()");
        }

        // Cập nhật UI nếu cần
        SetupUI();
    }

    // Hiển thị thông tin debug (chỉ để kiểm tra)
    public void ShowDebugInfo()
    {
        if (gameProgress != null)
        {
            string info = "===== DEBUG INFO =====\n";
            info += "Current Map: " + gameProgress.currentMap + "\n";

            // Hiển thị maps đã mở khóa
            info += "Maps đã mở khóa: ";
            for (int i = 1; i < 7; i++)
            {
                if (gameProgress.IsMapUnlocked(i))
                    info += i + ", ";
            }
            info += "\n";

            // Hiển thị maps đã hoàn thành
            info += "Maps đã hoàn thành: ";
            for (int i = 1; i < 7; i++)
            {
                if (gameProgress.IsMapCompleted(i))
                    info += i + ", ";
            }
            info += "\n";

            for (int i = 0; i < 6; i++)
            {
                info += "Map " + (i + 1) + ": " +
                    "Coins=" + gameProgress.collectedCoins[i] + "/" + gameProgress.requiredCoins[i] + ", " +
                    "DeadKnight=" + gameProgress.defeatedDeadKnight[i] + ", " +
                    "Ashe=" + gameProgress.defeatedAshe[i] + ", " +
                    "Zombie=" + gameProgress.defeatedZombie[i] + "\n";
            }

            Debug.Log(info);
        }
        else
        {
            Debug.LogError("GameProgress không tồn tại!");
        }
    }

    // Reset tiến trình (dùng cho testing)
    public void ResetProgress()
    {
        if (gameProgress != null)
        {
            gameProgress.ResetAllProgress();
            Debug.Log("Đã reset tiến trình game thông qua GameProgress");
        }
        else
        {
            Debug.LogError("GameProgress không tồn tại, không thể reset!");
        }

        // Cập nhật lại UI sau khi reset
        SetupUI();
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinnerButton : MonoBehaviour
{
    [Header("Main Controls")]
    public Button directExitButton;  // Nút thoát trực tiếp

    [Header("Menu Panel & Buttons")]
    public Button resetGameButton;  // Nút reset game

    [Header("Home Scene Settings")]
    public string homeSceneName = "HomeScene";  // Tên scene home

    [Header("Debug Options")]
    public bool showDebugLogs = true;  // Tùy chọn hiển thị log debug

    void Start()
    {
        // Kiểm tra tham chiếu
        if (resetGameButton == null) DebugLog("ResetGameButton reference is missing!", true);

        // Thiết lập listener trực tiếp cho nút Direct Exit nếu có
        if (directExitButton != null)
        {
            directExitButton.onClick.RemoveAllListeners();
            directExitButton.onClick.AddListener(ExitGame);
            DebugLog("Direct Exit Button listener set up");
        }

        // Thiết lập listener trực tiếp cho nút Reset Game
        if (resetGameButton != null)
        {
            resetGameButton.onClick.RemoveAllListeners();
            resetGameButton.onClick.AddListener(ResetGame);
            DebugLog("Reset Game Button listener set up");
        }
    }

    // Hàm ghi log có điều kiện
    private void DebugLog(string message, bool isError = false)
    {
        if (showDebugLogs || isError)
        {
            if (isError)
                Debug.LogError("[WinnerButton] " + message);
            else
                Debug.Log("[WinnerButton] " + message);
        }
    }

    // Phương thức để reset game (trực tiếp không cần xác nhận)
    public void ResetGame()
    {
        DebugLog("Resetting game completely...");

        // Đặt Time.timeScale thành 1 để đảm bảo game chạy bình thường khi load scene
        Time.timeScale = 1f;

        // Reset dữ liệu game hoàn toàn
        ResetAllGameData();

        // Load scene Home
        DebugLog("Loading home scene: " + homeSceneName);
        SceneManager.LoadScene(homeSceneName);
    }

    // Reset toàn bộ dữ liệu game
    private void ResetAllGameData()
    {
        DebugLog("Resetting all game data...");

        // 1. Sử dụng GameProgress nếu có
        ResetGameProgressManager();

        // 2. Sử dụng GameManager nếu có
        ResetGameManager();

        // 3. Xóa PlayerPrefs để đảm bảo xóa mọi dữ liệu
        ResetPlayerPrefs();

        DebugLog("All game data has been reset successfully!");
    }

    // Reset GameProgress (script quản lý tiến trình map)
    private void ResetGameProgressManager()
    {
        GameProgress gameProgress = FindObjectOfType<GameProgress>();
        if (gameProgress != null)
        {
            DebugLog("Found GameProgress - calling ResetProgress method");
            gameProgress.ResetProgress();
        }
        else
        {
            DebugLog("GameProgress not found, trying to reset PlayerPrefs directly");

            // Xóa tất cả các PlayerPrefs liên quan đến GameProgress
            // CurrentMap
            PlayerPrefs.DeleteKey("CurrentMap");

            // Map Unlocked & Completed
            for (int i = 0; i < 7; i++)
            {
                PlayerPrefs.DeleteKey("MapUnlocked_" + i);
                PlayerPrefs.DeleteKey("MapCompleted_" + i);
                PlayerPrefs.DeleteKey("CollectedCoins_" + i);
                PlayerPrefs.DeleteKey("CollectedChests_" + i);
                PlayerPrefs.DeleteKey("DefeatedDeadKnight_" + i);
                PlayerPrefs.DeleteKey("DefeatedAshe_" + i);
                PlayerPrefs.DeleteKey("DefeatedZombie_" + i);
            }
        }
    }

    // Reset GameManager (script quản lý dữ liệu game)
    private void ResetGameManager()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            DebugLog("Found GameManager - trying to reset its data");

            // Lưu ý: GameManager không có phương thức reset public
            // Xóa PlayerPrefs mà GameManager sử dụng
            PlayerPrefs.DeleteKey("GameProgress");
            PlayerPrefs.DeleteKey("PlayerItems");
        }
    }

    // Reset PlayerPrefs (xóa tất cả dữ liệu lưu trữ)
    private void ResetPlayerPrefs()
    {
        DebugLog("Deleting all PlayerPrefs...");

        // Xóa tất cả PlayerPrefs - cách triệt để nhất
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        DebugLog("All PlayerPrefs deleted");
    }

    // Thoát game trực tiếp không cần xác nhận
    public void ExitGame()
    {
        DebugLog("Exiting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
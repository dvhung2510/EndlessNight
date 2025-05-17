using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Main Controls")]
    public Button settingsButton;    // Nút Settings chính
    public Button pauseButton;
    public Button continueButton;

    [Header("Menu Panel & Buttons")]
    public GameObject menuPanel;
    private CanvasGroup menuPanelGroup;
    public Button playerInfoButton;
    public Button soundButton;
    public Button exitButton;

    [Header("Function Panels")]
    public GameObject playerInfoPanel;
    private CanvasGroup playerInfoPanelGroup;

    public GameObject soundPanel;
    private CanvasGroup soundPanelGroup;

    [Header("Exit Confirmation")]
    public GameObject exitConfirmPanel;   // ExitConfirmPanel
    public GameObject buttonPanel_Answer; // ButtonPanel_Answer
    public Button yesButton;             // YesButton
    public Button noButton;              // NoButton

    [Header("Home Scene Settings")]
    public string homeSceneName = "HomeScene";

    private bool isGamePaused = false;
    private Vector3 pauseButtonPosition;

    public Button testButton;

    void Start()
    {
        // Kiểm tra tất cả các tham chiếu
        if (menuPanel == null) Debug.LogError("MenuPanel reference is missing!");
        if (settingsButton == null) Debug.LogError("SettingsButton reference is missing!");
        if (playerInfoButton == null) Debug.LogError("PlayerInfoButton reference is missing!");
        if (soundButton == null) Debug.LogError("SoundButton reference is missing!");
        if (exitButton == null) Debug.LogError("ExitButton reference is missing!");
        if (pauseButton == null) Debug.LogError("PauseButton reference is missing!");
        if (continueButton == null) Debug.LogError("ContinueButton reference is missing!");

        // Kiểm tra các panel
        if (playerInfoPanel == null) Debug.LogError("PlayerInfoPanel reference is missing!");
        if (soundPanel == null) Debug.LogError("SoundPanel reference is missing!");

        // Kiểm tra tham chiếu cho exit confirmation
        if (exitConfirmPanel == null) Debug.LogError("ExitConfirmPanel reference is missing!");
        if (buttonPanel_Answer == null) Debug.LogError("ButtonPanel_Answer reference is missing!");
        if (yesButton == null) Debug.LogError("YesButton reference is missing!");
        if (noButton == null) Debug.LogError("NoButton reference is missing!");

        // Lấy hoặc thêm CanvasGroup cho các panel
        menuPanelGroup = GetOrAddCanvasGroup(menuPanel);
        playerInfoPanelGroup = GetOrAddCanvasGroup(playerInfoPanel);
        soundPanelGroup = GetOrAddCanvasGroup(soundPanel);

        // Debug để kiểm tra tham chiếu
        Debug.Log("MenuPanel: " + (menuPanel != null ? "Found" : "NULL"));

        // Thiết lập trạng thái ban đầu
        menuPanel.SetActive(false);  // Ẩn ban đầu, sẽ hiển thị khi click
        playerInfoPanel.SetActive(false);
        soundPanel.SetActive(false);

        // Ẩn panel xác nhận thoát khi bắt đầu
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);

        // Nếu có nút pause, lưu vị trí và thiết lập nút continue
        if (pauseButton != null && continueButton != null)
        {
            pauseButtonPosition = pauseButton.transform.position;
            continueButton.transform.position = pauseButtonPosition;
            continueButton.gameObject.SetActive(false);

            // Thêm listener cho pauseButton
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(PauseGame);
            Debug.Log("PauseButton listener set up successfully");

            // Thêm listener cho continueButton
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueGame);
            Debug.Log("ContinueButton listener set up successfully");
        }

        // Đảm bảo gán lại các listener cho các nút
        settingsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.AddListener(ToggleSettingsMenu);

        playerInfoButton.onClick.RemoveAllListeners();
        playerInfoButton.onClick.AddListener(ShowPlayerInfoPanel);

        soundButton.onClick.RemoveAllListeners();
        soundButton.onClick.AddListener(ShowSoundPanel);

        // Thiết lập listener cho nút Exit trong menu - đổi thành hiển thị panel xác nhận
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(ShowExitConfirmation);

        // Thiết lập listener cho các nút trong panel xác nhận thoát
        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(ConfirmExit);
            Debug.Log("YesButton listener set up successfully");
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(CancelExit);
            Debug.Log("NoButton listener set up successfully");
        }

        // Thiết lập listener cho các nút đóng
        SetCloseButtonListeners();

        // Thêm TestButton
        if (testButton != null)
        {
            testButton.onClick.RemoveAllListeners();
            testButton.onClick.AddListener(() => {
                menuPanel.SetActive(true);
                Debug.Log("MenuPanel activated by TestButton");
            });
        }
    }

    // Thêm hàm để thiết lập các nút đóng
    private void SetCloseButtonListeners()
    {
        // Thiết lập listener cho các nút đóng trong panel
        if (playerInfoPanel != null)
        {
            Button playerInfoCloseButton = playerInfoPanel.GetComponentInChildren<Button>();
            if (playerInfoCloseButton != null)
            {
                playerInfoCloseButton.onClick.RemoveAllListeners();
                playerInfoCloseButton.onClick.AddListener(ClosePlayerInfoPanel);
                Debug.Log("PlayerInfoPanel close button listener set");
            }
        }

        if (soundPanel != null)
        {
            Button soundCloseButton = soundPanel.GetComponentInChildren<Button>();
            if (soundCloseButton != null)
            {
                soundCloseButton.onClick.RemoveAllListeners();
                soundCloseButton.onClick.AddListener(CloseSoundPanel);
                Debug.Log("SoundPanel close button listener set");
            }
        }
    }

    // Thêm hàm này để đảm bảo các panel có CanvasGroup
    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        if (obj == null) return null;

        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = obj.AddComponent<CanvasGroup>();
        }
        return group;
    }

    // Thêm hàm để debug
    void Update()
    {
        // Kiểm tra nếu nhấn phím T
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Test key pressed");

            // Kiểm tra null
            if (menuPanel != null)
            {
                bool isActive = menuPanel.activeInHierarchy;
                menuPanel.SetActive(!isActive);
                Debug.Log("MenuPanel toggled by key T. Active: " + menuPanel.activeInHierarchy);
            }
            else
            {
                Debug.LogError("MenuPanel is null in T key test!");
            }
        }

        // Thêm phím P để test chức năng pause/continue
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P key pressed - Testing pause/continue functionality");
            if (isGamePaused)
            {
                ContinueGame();
            }
            else
            {
                PauseGame();
            }
        }

        // Thêm phím Escape để thoát game trực tiếp (tùy chọn)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (exitConfirmPanel != null && exitConfirmPanel.activeInHierarchy)
            {
                // Nếu panel xác nhận thoát đang mở, đóng nó
                CancelExit();
            }
            else if (menuPanel != null && menuPanel.activeInHierarchy)
            {
                // Nếu menu đang mở, đóng menu
                menuPanel.SetActive(false);
            }
            else
            {
                // Nếu menu đang đóng, mở menu
                if (menuPanel != null)
                {
                    menuPanel.SetActive(true);
                }
            }
        }

        // Debug chuột để kiểm tra click
        if (Input.GetMouseButtonDown(0) && pauseButton != null)
        {
            Vector3 mousePos = Input.mousePosition;
            RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();
            if (pauseRect != null)
            {
                // Kiểm tra xem chuột có nằm trong vùng của button không
                if (RectTransformUtility.RectangleContainsScreenPoint(pauseRect, mousePos, Camera.main))
                {
                    Debug.Log("Mouse clicked on pauseButton area!");
                }
            }
        }
    }

    public void ToggleSettingsMenu()
    {
        Debug.Log("ToggleSettings called");
        Debug.Log("MenuPanel is null: " + (menuPanel == null));

        if (menuPanel == null)
        {
            Debug.LogError("MenuPanel reference is null!");
            return;
        }

        bool isActive = menuPanel.activeInHierarchy;
        menuPanel.SetActive(!isActive);

        Debug.Log("MenuPanel active: " + menuPanel.activeInHierarchy);
    }

    // Sửa lại phương thức ContinueGame
    public void ContinueGame()
    {
        Debug.Log("ContinueGame() called");
        isGamePaused = false;
        Time.timeScale = 1f; // Khôi phục thời gian

        // Hiển thị nút pause, ẩn nút continue
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
            Debug.Log("Pause button activated");
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            Debug.Log("Continue button deactivated");
        }

        // Đảm bảo nút Settings có thể nhấp được
        if (settingsButton != null) settingsButton.interactable = true;

        // Ẩn các panel nếu có
        if (menuPanel != null && menuPanel.activeInHierarchy)
            menuPanel.SetActive(false);

        if (exitConfirmPanel != null && exitConfirmPanel.activeInHierarchy)
            exitConfirmPanel.SetActive(false);

        // Log để kiểm tra
        Debug.Log("Game continued. Settings button interactable: " +
                  (settingsButton != null ? settingsButton.interactable.ToString() : "NULL"));
    }

    // Sửa lại phương thức PauseGame
    public void PauseGame()
    {
        Debug.Log("PauseGame() called");
        isGamePaused = true;
        Time.timeScale = 0f; // Dừng thời gian

        // Ẩn nút pause, hiển thị nút continue
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
            Debug.Log("Pause button deactivated");
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            Debug.Log("Continue button activated");
        }

        // Đảm bảo menu panel đóng khi pause
        if (menuPanel != null) menuPanel.SetActive(false);

        // Ẩn panel xác nhận thoát nếu đang mở
        if (exitConfirmPanel != null) exitConfirmPanel.SetActive(false);

        // Log để kiểm tra
        Debug.Log("Game paused. Menu panel active: " +
                  (menuPanel != null ? menuPanel.activeInHierarchy.ToString() : "NULL"));
    }

    // Phương thức để reset game (trực tiếp không cần xác nhận)
    public void ResetGame()
    {
        Debug.Log("Resetting game...");

        // Đặt Time.timeScale thành 1 để đảm bảo game chạy bình thường khi load scene
        Time.timeScale = 1f;

        // Reset PlayerPrefs nếu cần
        ResetGameData();

        // Load scene Home
        SceneManager.LoadScene(homeSceneName);
    }

    // Phương thức reset dữ liệu game
    private void ResetGameData()
    {
        // Reset các PlayerPrefs liên quan đến tiến trình game
        // Ví dụ:
        PlayerPrefs.DeleteKey("UnlockedMaps");
        PlayerPrefs.DeleteKey("PlayerLevel");
        PlayerPrefs.DeleteKey("PlayerExp");
        PlayerPrefs.DeleteKey("PlayerHealth");
        PlayerPrefs.DeleteKey("PlayerMana");
        // ... Thêm các key khác nếu cần

        // Có thể giữ lại các cài đặt như âm thanh nếu muốn
        // Lưu lại sau khi xóa
        PlayerPrefs.Save();

        Debug.Log("Game data has been reset");
    }

    public void ShowPlayerInfoPanel()
    {
        Debug.Log("ShowPlayerInfoPanel called");

        // Ẩn tất cả các panel
        HideAllPanels();

        // Hiển thị player info panel
        if (playerInfoPanel != null)
        {
            playerInfoPanel.SetActive(true);

            if (playerInfoPanelGroup != null)
            {
                playerInfoPanelGroup.alpha = 1;
                playerInfoPanelGroup.interactable = true;
                playerInfoPanelGroup.blocksRaycasts = true;
            }

            // Ẩn menu panel
            if (menuPanel != null) menuPanel.SetActive(false);

            // Cập nhật thông tin người chơi nếu có
            PlayerInfoManager playerInfoManager = FindObjectOfType<PlayerInfoManager>();
            if (playerInfoManager != null)
            {
                playerInfoManager.UpdatePlayerInfo();
            }

            Debug.Log("Player info panel shown - Active: " + playerInfoPanel.activeInHierarchy);
        }
    }

    public void ClosePlayerInfoPanel()
    {
        if (playerInfoPanel != null)
        {
            playerInfoPanel.SetActive(false);
            Debug.Log("Player info panel closed");
        }
    }

    public void ShowSoundPanel()
    {
        Debug.Log("ShowSoundPanel called");

        // Ẩn tất cả các panel
        HideAllPanels();

        if (soundPanel != null)
        {
            soundPanel.SetActive(true);

            if (soundPanelGroup != null)
            {
                soundPanelGroup.alpha = 1;
                soundPanelGroup.interactable = true;
                soundPanelGroup.blocksRaycasts = true;
            }

            // Ẩn menu panel
            if (menuPanel != null) menuPanel.SetActive(false);

            // Tải cài đặt âm thanh nếu có
            SoundManager soundManager = FindObjectOfType<SoundManager>();
            if (soundManager != null)
            {
                soundManager.LoadVolumeSettings();
            }

            Debug.Log("Sound panel shown - Active: " + soundPanel.activeInHierarchy);
        }
    }

    public void CloseSoundPanel()
    {
        if (soundPanel != null)
        {
            soundPanel.SetActive(false);

            // Lưu cài đặt âm thanh nếu có
            SoundManager soundManager = FindObjectOfType<SoundManager>();
            if (soundManager != null)
            {
                soundManager.SaveVolumeSettings();
            }

            Debug.Log("Sound panel closed");
        }
    }

    // THÊM MỚI: Phương thức hiển thị panel xác nhận thoát
    public void ShowExitConfirmation()
    {
        Debug.Log("Showing exit confirmation panel");

        // Ẩn menu panel nếu đang hiển thị
        if (menuPanel != null)
            menuPanel.SetActive(false);

        // Hiển thị panel xác nhận
        if (exitConfirmPanel != null)
        {
            exitConfirmPanel.SetActive(true);
            Debug.Log("Exit confirmation panel activated");
        }
        else
        {
            Debug.LogError("exitConfirmPanel is null!");
        }
    }

    // THÊM MỚI: Phương thức xác nhận thoát khi nhấn nút Yes
    public void ConfirmExit()
    {
        Debug.Log("Exit confirmed - Quitting game");

        // Thoát game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // THÊM MỚI: Phương thức hủy thoát khi nhấn nút No
    public void CancelExit()
    {
        Debug.Log("Exit canceled");

        // Ẩn panel xác nhận
        if (exitConfirmPanel != null)
            exitConfirmPanel.SetActive(false);

        // Hiển thị lại menu panel
        if (menuPanel != null)
            menuPanel.SetActive(true);
    }

    // Điều chỉnh hàm ShowPanel và HidePanel
    void ShowPanel(CanvasGroup group)
    {
        if (group == null) return;

        group.gameObject.SetActive(true);
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    void HidePanel(CanvasGroup group)
    {
        if (group == null) return;

        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);
    }

    public void HideAllPanels()
    {
        if (playerInfoPanelGroup != null) HidePanel(playerInfoPanelGroup);
        if (soundPanelGroup != null) HidePanel(soundPanelGroup);
        if (exitConfirmPanel != null) exitConfirmPanel.SetActive(false);
    }

    // Thêm một hình ảnh gỡ lỗi để kiểm tra Settings Button
    void OnValidate()
    {
        Debug.Log("Settings Button reference: " + (settingsButton != null ? "Valid" : "NULL"));
        Debug.Log("Pause Button reference: " + (pauseButton != null ? "Valid" : "NULL"));
        Debug.Log("Continue Button reference: " + (continueButton != null ? "Valid" : "NULL"));
        Debug.Log("Exit Confirm Panel reference: " + (exitConfirmPanel != null ? "Valid" : "NULL"));
        Debug.Log("Yes Button reference: " + (yesButton != null ? "Valid" : "NULL"));
        Debug.Log("No Button reference: " + (noButton != null ? "Valid" : "NULL"));
    }
}
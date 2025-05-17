using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject howToPlayPanel;
    public GameObject settingsPanel; // Thêm panel cài đặt

    [Header("Buttons")]
    public Button playButton;
    public Button howToPlayButton;
    public Button settingsButton; // Thêm nút cài đặt
    public Button exitButton;

    [Header("Settings Panel Elements")]
    public Slider musicVolumeSlider; // Slider điều chỉnh âm lượng nhạc nền
    public Button closeSettingsButton; // Nút đóng panel cài đặt
    public Button applySettingsButton; // Nút áp dụng cài đặt

    [Header("Scene Loading")]
    public string homeSceneName = "HomeScene"; // Tên scene bạn muốn load trực tiếp

    // Tham chiếu đến Music Manager
    private SimpleMusicManager musicManager;

    void Start()
    {
        // Tìm MusicManager
        musicManager = FindObjectOfType<SimpleMusicManager>();
        if (musicManager == null)
        {
            Debug.LogWarning("Không tìm thấy SimpleMusicManager trong scene!");
        }

        // Đảm bảo các panel ban đầu
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Thiết lập giá trị slider nếu có music manager
        if (musicVolumeSlider != null && musicManager != null)
        {
            musicVolumeSlider.value = musicManager.GetVolume();

            // Thêm listener cho slider
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        // Thiết lập các nút
        SetupButtons();
    }

    // Xử lý khi slider âm lượng thay đổi
    private void OnMusicVolumeChanged(float volume)
    {
        if (musicManager != null)
        {
            musicManager.SetVolume(volume);
        }
    }

    // Thiết lập các nút
    private void SetupButtons()
    {
        // Nút Play - load trực tiếp HomeScene
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => {
                PlayButtonClickSound();
                PlayGame();
            });
        }

        // Nút How to Play
        if (howToPlayButton != null)
        {
            howToPlayButton.onClick.RemoveAllListeners();
            howToPlayButton.onClick.AddListener(() => {
                PlayButtonClickSound();
                ShowHowToPlay();
            });
        }

        // Nút Settings (Cài đặt)
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => {
                PlayButtonClickSound();
                ShowSettings();
            });
        }

        // Nút đóng panel cài đặt
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveAllListeners();
            closeSettingsButton.onClick.AddListener(() => {
                PlayButtonClickSound();
                CloseSettings();
            });
        }

        // Nút áp dụng cài đặt
        if (applySettingsButton != null)
        {
            applySettingsButton.onClick.RemoveAllListeners();
            applySettingsButton.onClick.AddListener(() => {
                PlayButtonClickSound();
                ApplySettings();
                CloseSettings();
            });
        }

        // Nút Exit
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => {
                PlayButtonClickSound();
                ExitGame();
            });
        }
    }

    // Phát âm thanh khi nhấn nút
    private void PlayButtonClickSound()
    {
        // Tìm AudioManager để phát âm thanh nút nhấn
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }

    // Phương thức này sẽ load trực tiếp HomeScene
    public void PlayGame()
    {
        Debug.Log("Đang tải HomeScene...");
        // Lưu lại thông tin rằng đây là lần đầu vào game
        PlayerPrefs.SetInt("CurrentMap", 0); // 0 là index của HomeScene
        PlayerPrefs.Save();
        // Load HomeScene
        SceneManager.LoadScene(homeSceneName);
    }

    // Hiển thị panel hướng dẫn chơi
    public void ShowHowToPlay()
    {
        // Ẩn menu chính
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        // Hiển thị panel how to play
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }
    }

    // Đóng panel hướng dẫn chơi
    public void CloseHowToPlay()
    {
        // Ẩn panel how to play
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
        // Hiển thị lại menu chính
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    // Hiển thị panel cài đặt
    public void ShowSettings()
    {
        // Ẩn menu chính
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        // Hiển thị panel cài đặt
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            // Cập nhật giá trị slider khi mở panel
            if (musicVolumeSlider != null && musicManager != null)
            {
                musicVolumeSlider.value = musicManager.GetVolume();
            }
        }
    }

    // Đóng panel cài đặt
    public void CloseSettings()
    {
        // Ẩn panel cài đặt
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        // Hiển thị lại menu chính
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    // Áp dụng cài đặt
    public void ApplySettings()
    {
        // Lưu cài đặt âm lượng nhạc nền
        if (musicManager != null && musicVolumeSlider != null)
        {
            musicManager.SetVolume(musicVolumeSlider.value);
        }

        // Lưu các cài đặt khác nếu có
        PlayerPrefs.Save();

        Debug.Log("Đã áp dụng cài đặt.");
    }

    // Thoát game
    public void ExitGame()
    {
        Debug.Log("Exiting Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
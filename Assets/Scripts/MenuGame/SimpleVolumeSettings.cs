using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleVolumeSettings : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private GameObject targetPanelToClose; // Panel cần đóng (có thể là parent hoặc panel chỉ định)
    [SerializeField] private float defaultMusicVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 0.8f;

    // Giá trị hiện tại
    private float musicVolume;
    private float sfxVolume;

    // Tham chiếu đến manager
    private SimpleMusicManager musicManager;
    private AudioManager audioManager;

    private void Awake()
    {
        // Tìm target panel nếu chưa được chỉ định
        if (targetPanelToClose == null)
        {
            targetPanelToClose = transform.parent.gameObject;
        }
    }

    private void Start()
    {
        // Tìm manager
        FindManagers();

        // Kiểm tra và thông báo các tham chiếu UI
        ValidateUIReferences();

        // Thiết lập các event listener
        SetupListeners();

        // Khởi tạo giá trị mặc định
        musicVolume = defaultMusicVolume;
        sfxVolume = defaultSFXVolume;

        // Tải và áp dụng cài đặt
        LoadSettings();
    }

    private void FindManagers()
    {
        // Tìm music manager
        musicManager = SimpleMusicManager.Instance;
        if (musicManager == null)
        {
            Debug.LogWarning("SimpleVolumeSettings: Không tìm thấy SimpleMusicManager.Instance!");
        }

        // Tìm audio manager
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogWarning("SimpleVolumeSettings: Không tìm thấy AudioManager.Instance!");
        }
    }

    private void ValidateUIReferences()
    {
        // Kiểm tra các tham chiếu UI
        if (musicVolumeSlider == null) Debug.LogError("SimpleVolumeSettings: Thiếu tham chiếu musicVolumeSlider!");
        if (sfxVolumeSlider == null) Debug.LogError("SimpleVolumeSettings: Thiếu tham chiếu sfxVolumeSlider!");
        if (musicVolumeText == null) Debug.LogError("SimpleVolumeSettings: Thiếu tham chiếu musicVolumeText!");
        if (sfxVolumeText == null) Debug.LogError("SimpleVolumeSettings: Thiếu tham chiếu sfxVolumeText!");
        if (applyButton == null) Debug.LogError("SimpleVolumeSettings: Thiếu tham chiếu applyButton!");
        if (closeButton == null) Debug.LogError("SimpleVolumeSettings: Thiếu tham chiếu closeButton!");
    }

    private void SetupListeners()
    {
        // Slider âm lượng nhạc nền
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners(); // Xóa listener cũ để tránh trùng lặp
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        // Slider âm lượng hiệu ứng
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners(); // Xóa listener cũ để tránh trùng lặp
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Nút áp dụng
        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners(); // Xóa listener cũ để tránh trùng lặp
            applyButton.onClick.AddListener(SaveSettings);
        }

        // Nút đóng
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners(); // Xóa listener cũ để tránh trùng lặp
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    // Xử lý khi slider âm lượng nhạc thay đổi
    private void OnMusicVolumeChanged(float value)
    {
        musicVolume = value;

        // Cập nhật text
        if (musicVolumeText != null)
        {
            musicVolumeText.text = $"Nhạc nền: {Mathf.RoundToInt(value * 100)}%";
        }

        // Áp dụng ngay lập tức cho âm lượng nhạc nền
        if (musicManager != null)
        {
            musicManager.SetVolume(value);
        }
        else
        {
            // Lưu trực tiếp vào PlayerPrefs nếu không có manager
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }

    // Xử lý khi slider âm lượng SFX thay đổi
    private void OnSFXVolumeChanged(float value)
    {
        sfxVolume = value;

        // Cập nhật text
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"Hiệu ứng: {Mathf.RoundToInt(value * 100)}%";
        }

        // Áp dụng cho âm lượng hiệu ứng
        if (audioManager != null)
        {
            audioManager.masterVolume = value;
            audioManager.UpdateAllVolumes();

            // Phát âm thanh để người dùng nghe thấy sự thay đổi
            audioManager.PlayButtonClick();
        }
        else
        {
            // Lưu trực tiếp vào PlayerPrefs nếu không có manager
            PlayerPrefs.SetFloat("SFXVolume", value);
        }
    }

    // Tải cài đặt
    private void LoadSettings()
    {
        // Tải giá trị từ PlayerPrefs (ưu tiên) hoặc từ Manager

        // Âm lượng nhạc nền
        if (musicVolumeSlider != null)
        {
            // Ưu tiên lấy từ PlayerPrefs trước
            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
            }
            else if (musicManager != null)
            {
                // Nếu không có trong PlayerPrefs thì lấy từ manager
                musicVolume = musicManager.GetVolume();
            }
            else
            {
                // Nếu không có cả hai, sử dụng giá trị mặc định
                musicVolume = defaultMusicVolume;
            }

            // Cập nhật UI
            musicVolumeSlider.value = musicVolume;
            OnMusicVolumeChanged(musicVolume);
        }

        // Âm lượng hiệu ứng
        if (sfxVolumeSlider != null)
        {
            // Ưu tiên lấy từ PlayerPrefs trước
            if (PlayerPrefs.HasKey("SFXVolume"))
            {
                sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
            }
            else if (audioManager != null)
            {
                // Nếu không có trong PlayerPrefs thì lấy từ manager
                sfxVolume = audioManager.masterVolume;
            }
            else
            {
                // Nếu không có cả hai, sử dụng giá trị mặc định
                sfxVolume = defaultSFXVolume;
            }

            // Cập nhật UI
            sfxVolumeSlider.value = sfxVolume;
            OnSFXVolumeChanged(sfxVolume);
        }
    }

    // Lưu cài đặt
    public void SaveSettings()
    {
        // Lưu cài đặt âm lượng nhạc nền
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        // Lưu cài đặt âm lượng hiệu ứng
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

        // Áp dụng cài đặt vào manager
        if (musicManager != null)
        {
            musicManager.SetVolume(musicVolume);
        }

        if (audioManager != null)
        {
            audioManager.masterVolume = sfxVolume;
            audioManager.UpdateAllVolumes();
        }

        // Lưu ngay lập tức
        PlayerPrefs.Save();

        Debug.Log("Đã lưu cài đặt âm lượng: Nhạc nền = " + musicVolume + ", Hiệu ứng = " + sfxVolume);
    }

    // Đóng panel và lưu cài đặt
    public void ClosePanel()
    {
        // Lưu cài đặt trước khi đóng
        SaveSettings();

        // Đóng panel theo thứ tự ưu tiên
        if (targetPanelToClose != null)
        {
            // Nếu đã chỉ định panel cụ thể để đóng
            targetPanelToClose.SetActive(false);
            Debug.Log("Đã đóng panel chỉ định");
        }
        else
        {
            // Tìm parent panel và ẩn nó
            Transform parent = transform.parent;
            if (parent != null)
            {
                parent.gameObject.SetActive(false);
                Debug.Log("Đã đóng panel cha");
            }
            else
            {
                // Nếu không có parent, ẩn chính nó
                gameObject.SetActive(false);
                Debug.Log("Đã đóng panel hiện tại");
            }
        }
    }

    // Phương thức này có thể được gọi từ bên ngoài (ví dụ: từ MenuController)
    public void ResetToDefaults()
    {
        // Đặt lại về giá trị mặc định
        if (musicVolumeSlider != null) musicVolumeSlider.value = defaultMusicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = defaultSFXVolume;

        // Lưu cài đặt mặc định
        SaveSettings();

        Debug.Log("Đã đặt lại cài đặt âm lượng về mặc định");
    }
}
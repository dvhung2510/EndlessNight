using UnityEngine;

public class SimpleMusicManager : MonoBehaviour
{
    public static SimpleMusicManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip gameMusic; // Chỉ 1 bản nhạc cho cả game

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    // Key để lưu cài đặt âm lượng
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tạo AudioSource nếu chưa có
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }

            // Thiết lập thuộc tính
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            // Tải cài đặt âm lượng
            LoadVolumeSetting();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tự động phát nhạc khi khởi động game
        PlayMusic();
    }

    // Phát nhạc
    public void PlayMusic()
    {
        if (gameMusic == null)
        {
            Debug.LogWarning("Không có file nhạc được chỉ định!");
            return;
        }

        if (musicSource.clip != gameMusic || !musicSource.isPlaying)
        {
            musicSource.clip = gameMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    // Tạm dừng nhạc
    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    // Tiếp tục phát nhạc
    public void ResumeMusic()
    {
        if (!musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
        }
    }

    // Thiết lập âm lượng
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;

        // Lưu cài đặt
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.Save();
    }

    // Lấy giá trị âm lượng hiện tại
    public float GetVolume()
    {
        return musicVolume;
    }

    // Tải cài đặt âm lượng
    private void LoadVolumeSetting()
    {
        if (PlayerPrefs.HasKey(MUSIC_VOLUME_KEY))
        {
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY);
        }

        // Áp dụng âm lượng cho nguồn nhạc
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
}
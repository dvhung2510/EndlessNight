using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    // Audio Mixer để quản lý âm thanh
    public AudioMixer audioMixer;

    // Các slider điều chỉnh âm lượng
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    // Âm thanh UI (tùy chọn)
    public AudioSource uiAudioSource;
    public AudioClip buttonClickSound;
    public AudioClip openMenuSound;
    public AudioClip closeMenuSound;

    void Start()
    {
        // Thêm listener cho các slider
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        // Khởi tạo giá trị slider từ PlayerPrefs
        LoadVolumeSettings();
    }

    // Thiết lập âm lượng chung
    public void SetMasterVolume(float volume)
    {
        // Chuyển đổi đến dB (logarithmic)
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);

        // Play test sound
        PlayButtonClickSound();
    }

    // Thiết lập âm lượng nhạc nền
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);

        // Play test sound
        PlayButtonClickSound();
    }

    // Thiết lập âm lượng hiệu ứng
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);

        // Play test sound
        PlayButtonClickSound();
    }

    // Tải các cài đặt âm lượng từ PlayerPrefs
    public void LoadVolumeSettings()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // Áp dụng cài đặt
        SetMasterVolume(masterVolumeSlider.value);
        SetMusicVolume(musicVolumeSlider.value);
        SetSFXVolume(sfxVolumeSlider.value);
    }

    // Lưu các cài đặt âm lượng vào PlayerPrefs
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.Save();
    }

    // Phát âm thanh click button
    public void PlayButtonClickSound()
    {
        if (uiAudioSource != null && buttonClickSound != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    // Phát âm thanh mở menu
    public void PlayOpenMenuSound()
    {
        if (uiAudioSource != null && openMenuSound != null)
        {
            uiAudioSource.PlayOneShot(openMenuSound);
        }
    }

    // Phát âm thanh đóng menu
    public void PlayCloseMenuSound()
    {
        if (uiAudioSource != null && closeMenuSound != null)
        {
            uiAudioSource.PlayOneShot(closeMenuSound);
        }
    }
}
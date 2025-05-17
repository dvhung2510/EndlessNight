using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }

    [Header("Player Sounds")]
    public SoundEffect footstepSound;
    public SoundEffect runSound;
    public SoundEffect jumpSound;
    public SoundEffect doubleJumpSound;
    public SoundEffect shootSound;
    public SoundEffect hurtSound;
    public SoundEffect deathSound;

    [Header("Enemy Sounds")]
    public SoundEffect enemySlashSound;
    public SoundEffect enemyHurtSound;
    public SoundEffect enemyDeathSound;

    [Header("Item Sounds")]
    public SoundEffect coinPickupSound;
    public SoundEffect healthPotionSound;

    [Header("UI Sounds")]
    public SoundEffect buttonClickSound;
    public SoundEffect levelCompleteSound;
    public SoundEffect gameOverSound;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    // Tham chiếu đến các AudioSource
    private Dictionary<string, AudioSource> soundSources = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Khởi tạo audio sources
            InitializeSounds();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeSounds()
    {
        // Khởi tạo âm thanh cho người chơi
        SetupSound(footstepSound);
        SetupSound(runSound);
        SetupSound(jumpSound);
        SetupSound(doubleJumpSound);
        SetupSound(shootSound);
        SetupSound(hurtSound);
        SetupSound(deathSound);

        // Khởi tạo âm thanh cho kẻ thù
        SetupSound(enemySlashSound);
        SetupSound(enemyHurtSound);
        SetupSound(enemyDeathSound);

        // Khởi tạo âm thanh cho item
        SetupSound(coinPickupSound);
        SetupSound(healthPotionSound);

        // Khởi tạo âm thanh cho UI
        SetupSound(buttonClickSound);
        SetupSound(levelCompleteSound);
        SetupSound(gameOverSound);
    }

    private void SetupSound(SoundEffect sound)
    {
        if (sound.clip == null)
            return;

        // Tạo AudioSource mới
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = sound.clip;
        source.volume = sound.volume * masterVolume;
        source.pitch = sound.pitch;
        source.loop = sound.loop;
        source.playOnAwake = false;

        // Lưu reference
        sound.source = source;

        // Thêm vào dictionary
        if (!soundSources.ContainsKey(sound.name))
        {
            soundSources.Add(sound.name, source);
        }
        else
        {
            Debug.LogWarning($"Âm thanh trùng tên: {sound.name}");
        }
    }

    public void PlaySound(string soundName)
    {
        if (soundSources.TryGetValue(soundName, out AudioSource source))
        {
            if (!source.isPlaying)
            {
                source.Play();
            }
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy âm thanh: {soundName}");
        }
    }

    public void StopSound(string soundName)
    {
        if (soundSources.TryGetValue(soundName, out AudioSource source))
        {
            source.Stop();
        }
    }

    public void PlayFootstep()
    {
        if (footstepSound.source != null)
        {
            // Thêm một chút ngẫu nhiên cho âm thanh bước chân
            footstepSound.source.pitch = footstepSound.pitch * Random.Range(0.9f, 1.1f);
            footstepSound.source.Play();
        }
    }

    public void PlayRun()
    {
        if (runSound.source != null && !runSound.source.isPlaying)
        {
            runSound.source.Play();
        }
    }

    public void StopRun()
    {
        if (runSound.source != null && runSound.source.isPlaying)
        {
            runSound.source.Stop();
        }
    }

    public void PlayJump()
    {
        if (jumpSound.source != null)
        {
            jumpSound.source.Play();
        }
    }

    public void PlayDoubleJump()
    {
        if (doubleJumpSound.source != null)
        {
            doubleJumpSound.source.Play();
        }
    }

    public void PlayShoot()
    {
        if (shootSound.source != null)
        {
            shootSound.source.Play();
        }
    }

    public void PlayPlayerHurt()
    {
        if (hurtSound.source != null)
        {
            hurtSound.source.Play();
        }
    }

    public void PlayPlayerDeath()
    {
        if (deathSound.source != null)
        {
            deathSound.source.Play();
        }
    }

    public void PlayEnemySlash()
    {
        if (enemySlashSound.source != null)
        {
            enemySlashSound.source.Play();
        }
    }

    public void PlayEnemyHurt()
    {
        if (enemyHurtSound.source != null)
        {
            enemyHurtSound.source.Play();
        }
    }

    public void PlayEnemyDeath()
    {
        if (enemyDeathSound.source != null)
        {
            enemyDeathSound.source.Play();
        }
    }

    public void PlayCoinPickup()
    {
        if (coinPickupSound.source != null)
        {
            coinPickupSound.source.Play();
        }
    }

    public void PlayHealthPotion()
    {
        if (healthPotionSound.source != null)
        {
            healthPotionSound.source.Play();
        }
    }

    public void PlayButtonClick()
    {
        if (buttonClickSound.source != null)
        {
            buttonClickSound.source.Play();
        }
    }

    public void PlayLevelComplete()
    {
        if (levelCompleteSound.source != null)
        {
            levelCompleteSound.source.Play();
        }
    }

    public void PlayGameOver()
    {
        if (gameOverSound.source != null)
        {
            gameOverSound.source.Play();
        }
    }

    public void UpdateAllVolumes()
    {
        foreach (var source in soundSources.Values)
        {
            source.volume *= masterVolume;
        }
    }
}
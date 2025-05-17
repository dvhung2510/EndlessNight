using UnityEngine;

public class BossDefeatTracker : MonoBehaviour
{
    [Tooltip("Loại boss: DeadKnight, Ashe, hoặc Zombie")]
    public string bossType = "DeadKnight";
    private bool hasRegisteredDefeat = false;
    private static bool isQuitting = false;

    void Start()
    {
        Debug.Log("BossDefeatTracker đã được khởi tạo trên " + gameObject.name + " với bossType = " + bossType);
    }

    // Add this method to detect application quitting
    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    // Khi boss bị hủy (chết)
    private void OnDestroy()
    {
        if (Application.isPlaying && !isQuitting)
        {
            Debug.Log("OnDestroy được gọi cho " + gameObject.name);
            RegisterDefeat();
        }
    }

    // Đăng ký đã đánh bại boss
    public void RegisterDefeat()
    {
        if (hasRegisteredDefeat)
        {
            Debug.Log("Boss đã được đăng ký đánh bại trước đó, bỏ qua");
            return; // Tránh gọi nhiều lần
        }
        Debug.Log("Đang thử đăng ký đánh bại boss " + bossType);
        if (GameProgress.Instance != null)
        {
            int currentMap = GameProgress.Instance.currentMap;
            Debug.Log("Đang ghi nhận đánh bại " + bossType + " trong map " + currentMap);
            GameProgress.Instance.DefeatBoss(bossType, currentMap);
            hasRegisteredDefeat = true;
            // Làm mới thông tin sau khi đánh bại boss
            CheckMapCompletion(currentMap);
            // Thông báo cho MissionDisplay cập nhật
            NotifyMissionDisplay();
        }
        else
        {
            Debug.LogWarning("GameProgress.Instance là null! Thử ghi nhận sau 0.5 giây");
            // Lưu trạng thái vào PlayerPrefs để an toàn
            PlayerPrefs.SetInt("Defeated_" + bossType + "_CurrentMap", 1);
            PlayerPrefs.Save();
            // Thử lại sau một khoảng thời gian
            Invoke("RetryRegisterDefeat", 0.5f);
        }
    }

    // Thử lại đăng ký boss đã bị đánh bại
    private void RetryRegisterDefeat()
    {
        Debug.Log("Đang thử lại đăng ký đánh bại boss " + bossType);
        if (GameProgress.Instance != null)
        {
            int currentMap = GameProgress.Instance.currentMap;
            GameProgress.Instance.DefeatBoss(bossType, currentMap);
            hasRegisteredDefeat = true;
            Debug.Log("Đã ghi nhận thành công boss " + bossType + " bị đánh bại sau khi thử lại");
            CheckMapCompletion(currentMap);
            // Thông báo cho MissionDisplay cập nhật
            NotifyMissionDisplay();
        }
        else
        {
            Debug.LogError("GameProgress.Instance vẫn là null sau khi thử lại. Không thể ghi nhận!");
        }
    }

    // Kiểm tra xem map đã hoàn thành chưa sau khi đánh bại boss
    private void CheckMapCompletion(int mapIndex)
    {
        if (GameProgress.Instance != null)
        {
            bool hasEnoughItems = GameProgress.Instance.HasEnoughItems(mapIndex);
            Debug.Log("Sau khi đánh bại boss, map " + mapIndex + " đủ điều kiện hoàn thành: " + hasEnoughItems);
        }
    }

    // Thông báo cho MissionDisplay cập nhật
    private void NotifyMissionDisplay()
    {
        // Tìm tất cả MissionDisplay trong scene
        MissionDisplay[] displays = FindObjectsOfType<MissionDisplay>();
        if (displays.Length > 0)
        {
            foreach (MissionDisplay display in displays)
            {
                display.ForceUpdateMissionInfo();
            }
            Debug.Log("Đã thông báo cho " + displays.Length + " MissionDisplay cập nhật");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy MissionDisplay nào trong scene!");
        }
    }

    // Phương thức này có thể được gọi từ Animation Event hoặc Health
    public void OnBossDeath()
    {
        Debug.Log("OnBossDeath được gọi cho " + gameObject.name);
        RegisterDefeat();
    }
}
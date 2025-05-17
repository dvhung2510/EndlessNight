using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MapConnector : MonoBehaviour
{
    public enum TransitionDirection
    {
        Left,
        Right
    }

    [Tooltip("Hướng chuyển map")]
    public TransitionDirection transitionDirection;

    [Tooltip("Tên của scene sẽ chuyển đến")]
    public string targetSceneName;

    [Tooltip("Thời gian cho hiệu ứng fade")]
    public float fadeTime = 0.5f;

    [Tooltip("Yêu cầu điều kiện hoàn thành nhiệm vụ")]
    public bool requireCompletion = true;

    [Header("UI Notifications")]
    [Tooltip("Panel thông báo chưa hoàn thành nhiệm vụ")]
    public GameObject completionRequirementPanel;
    public Text requirementText;
    public float notificationDuration = 3f;

    private bool isTransitioning = false;
    private int currentMapIndex;

    private void Start()
    {
        Debug.Log(gameObject.name + ": MapConnector script đã được khởi tạo");

        // Lấy thông tin map hiện tại từ tên scene
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName.StartsWith("Map"))
        {
            string mapIndexStr = currentSceneName.Substring(3);
            if (int.TryParse(mapIndexStr, out int mapIndex))
            {
                currentMapIndex = mapIndex;

                // Cập nhật currentMap trong GameProgress
                if (GameProgress.instance != null)
                {
                    GameProgress.instance.SetCurrentMap(currentMapIndex);
                    Debug.Log("Đã cập nhật currentMap trong GameProgress: " + currentMapIndex);
                }
            }
        }
        else if (currentSceneName == "HomeScene")
        {
            currentMapIndex = 0;
        }

        // Ẩn panel thông báo nếu có
        if (completionRequirementPanel != null)
        {
            completionRequirementPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(gameObject.name + ": OnTriggerEnter2D với " + collision.gameObject.name);

        if (collision.CompareTag("Player") && !isTransitioning)
        {
            // Kiểm tra điều kiện hoàn thành nhiệm vụ
            bool canProceed = true;

            if (requireCompletion && currentMapIndex > 0 && GameProgress.instance != null)
            {
                // Kiểm tra tất cả các điều kiện
                canProceed = CheckAllRequirements();

                if (!canProceed)
                {
                    Debug.Log("Không thể qua map tiếp theo: Chưa hoàn thành nhiệm vụ");
                    DisplayCompletionMessage();
                    return;
                }
            }

            // Nếu qua được điều kiện hoàn thành, kiểm tra hướng di chuyển
            Debug.Log("Đã hoàn thành nhiệm vụ, kiểm tra hướng di chuyển");

            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 playerMovement = playerRb.linearVelocity.normalized;
                Debug.Log("Hướng di chuyển: " + playerMovement);

                bool correctDirection =
                    (transitionDirection == TransitionDirection.Right && playerMovement.x > 0) ||
                    (transitionDirection == TransitionDirection.Left && playerMovement.x < 0);

                if (correctDirection)
                {
                    Debug.Log("Hướng di chuyển đúng, bắt đầu chuyển scene");
                    isTransitioning = true;

                    // Lưu hướng vào cho map tiếp theo
                    if (transitionDirection == TransitionDirection.Left)
                    {
                        PlayerPrefs.SetString("EntryDirection", "Right");
                        Debug.Log("Đặt EntryDirection = Right cho map tiếp theo");
                    }
                    else
                    {
                        PlayerPrefs.SetString("EntryDirection", "Left");
                        Debug.Log("Đặt EntryDirection = Left cho map tiếp theo");
                    }
                    PlayerPrefs.SetInt("UseCustomSpawn", 1);
                    PlayerPrefs.Save();

                    StartCoroutine(SimpleSceneTransition());
                }
            }
        }
    }

    // Kiểm tra tất cả các yêu cầu hoàn thành
    private bool CheckAllRequirements()
    {
        if (GameProgress.instance == null) return false;

        int index = currentMapIndex - 1; // Chuyển sang 0-based
        if (index < 0 || index >= 6) return false;

        GameProgress gp = GameProgress.instance;

        // Kiểm tra coins và chests cho tất cả map
        bool hasEnoughCoins = gp.collectedCoins[index] >= gp.requiredCoins[index];
        bool hasEnoughChests = gp.collectedChests[index] >= gp.requiredChests[index];

        // Kiểm tra boss theo từng map cụ thể
        bool bossesDefeated = true;

        // Kiểm tra boss DeadKnight nếu cần
        if (gp.requireDeadKnight[index] && !gp.defeatedDeadKnight[index])
            bossesDefeated = false;

        // Kiểm tra boss Ashe nếu cần
        if (gp.requireAshe[index] && !gp.defeatedAshe[index])
            bossesDefeated = false;

        // Kiểm tra boss Zombie nếu cần
        if (gp.requireZombie[index] && !gp.defeatedZombie[index])
            bossesDefeated = false;

        // In log để debug
        Debug.Log("Kiểm tra yêu cầu Map " + currentMapIndex + ":");
        Debug.Log("- Coins: " + gp.collectedCoins[index] + "/" + gp.requiredCoins[index] + " (" + (hasEnoughCoins ? "ĐẠT" : "CHƯA ĐẠT") + ")");
        Debug.Log("- Chests: " + gp.collectedChests[index] + "/" + gp.requiredChests[index] + " (" + (hasEnoughChests ? "ĐẠT" : "CHƯA ĐẠT") + ")");

        if (gp.requireDeadKnight[index])
            Debug.Log("- DeadKnight: " + (gp.defeatedDeadKnight[index] ? "ĐÃ TIÊU DIỆT" : "CHƯA TIÊU DIỆT"));

        if (gp.requireAshe[index])
            Debug.Log("- Ashe: " + (gp.defeatedAshe[index] ? "ĐÃ TIÊU DIỆT" : "CHƯA TIÊU DIỆT"));

        if (gp.requireZombie[index])
            Debug.Log("- Zombie: " + (gp.defeatedZombie[index] ? "ĐÃ TIÊU DIỆT" : "CHƯA TIÊU DIỆT"));

        // Kết quả cuối cùng
        bool result = hasEnoughCoins && hasEnoughChests && bossesDefeated;

        Debug.Log("Kết quả kiểm tra: " + (result ? "ĐÃ HOÀN THÀNH" : "CHƯA HOÀN THÀNH"));

        return result;
    }

    private void DisplayCompletionMessage()
    {
        if (completionRequirementPanel != null && requirementText != null)
        {
            // Hiển thị panel
            completionRequirementPanel.SetActive(true);

            // Cập nhật text với thông tin chi tiết
            int index = currentMapIndex - 1;
            string message = "Để hoàn thành map, bạn cần:\n";

            if (GameProgress.instance != null)
            {
                GameProgress gp = GameProgress.instance;

                // Thêm yêu cầu coins nếu cần
                if (gp.requiredCoins[index] > 0)
                    message += "- Thu thập " + gp.collectedCoins[index] + "/" + gp.requiredCoins[index] + " Coins\n";

                // Thêm yêu cầu chests nếu cần
                if (gp.requiredChests[index] > 0)
                    message += "- Thu thập " + gp.collectedChests[index] + "/" + gp.requiredChests[index] + " Chests\n";

                // Thêm yêu cầu boss nếu cần
                if (gp.requireDeadKnight[index])
                    message += "- Đánh bại boss DeadKnight: " + (gp.defeatedDeadKnight[index] ? "Đã hoàn thành" : "Chưa hoàn thành") + "\n";

                if (gp.requireAshe[index])
                    message += "- Đánh bại boss Ashe: " + (gp.defeatedAshe[index] ? "Đã hoàn thành" : "Chưa hoàn thành") + "\n";

                if (gp.requireZombie[index])
                    message += "- Đánh bại boss Zombie: " + (gp.defeatedZombie[index] ? "Đã hoàn thành" : "Chưa hoàn thành");
            }
            else
            {
                message = "Hãy thu thập đủ vật phẩm để qua map tiếp theo!";
            }

            requirementText.text = message;

            // Tự động ẩn panel sau một khoảng thời gian
            StartCoroutine(HideNotificationAfterDelay());
        }
        else
        {
            // Fallback nếu không có UI
            Debug.Log("Hãy hoàn thành các nhiệm vụ để qua map tiếp theo!");
        }
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);

        if (completionRequirementPanel != null)
        {
            completionRequirementPanel.SetActive(false);
        }
    }

    private IEnumerator SimpleSceneTransition()
    {
        Debug.Log("Bắt đầu chuyển đến scene: " + targetSceneName);

        bool hasFadeManager = (FadeManager.instance != null);

        if (hasFadeManager)
        {
            yield return StartCoroutine(FadeManager.instance.FadeIn(fadeTime));
        }

        try
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi tải scene: " + e.Message);
        }

        yield return null;

        if (hasFadeManager)
        {
            yield return StartCoroutine(FadeManager.instance.FadeOut(fadeTime));
        }

        isTransitioning = false;
    }
}
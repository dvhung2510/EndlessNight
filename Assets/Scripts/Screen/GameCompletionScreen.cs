using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameCompletionScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject completionPanel;
    public TextMeshProUGUI congratulationsText;
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Settings")]
    public string mainMenuScene = "Scenes/MenuGame";
    public string firstMapScene = "Scenes/Map1";
    public float showDelay = 1.0f;

    private GameProgress gameProgress;

    private void Start()
    {
        // Lấy tham chiếu tới GameProgress instance
        gameProgress = GameProgress.Instance;

        // Ẩn panel khi bắt đầu
        if (completionPanel != null)
            completionPanel.SetActive(false);

        // Nếu đã hoàn thành game thì hiển thị sau một khoảng delay
        if (gameProgress != null && gameProgress.IsGameCompleted())
        {
            Invoke(nameof(ShowCompletionScreen), showDelay);
        }

        SetupButtons();
    }

    private void SetupButtons()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    public void ShowCompletionScreen()
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);

        if (congratulationsText != null)
        {
            congratulationsText.text = "🎉 Chúc mừng!\n\nBạn đã hoàn thành toàn bộ game!\n\nCảm ơn bạn đã chơi.";
        }
    }

    public void RestartGame()
    {
        if (gameProgress != null)
            gameProgress.ResetProgress();

        SceneManager.LoadScene(firstMapScene);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void Show()
    {
        ShowCompletionScreen();
    }
}

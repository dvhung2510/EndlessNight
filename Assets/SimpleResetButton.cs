using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleResetButton : MonoBehaviour
{
    // Tên scene menu chính
    public string menuSceneName = "HomeScene";

    private Button resetButton;

    void Start()
    {
        // Lấy component Button
        resetButton = GetComponent<Button>();

        // Thêm listener cho button
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetGame);
        }
    }

    // Phương thức reset game
    public void ResetGame()
    {
        // Reset vị trí spawn
        PlayerPrefs.SetInt("UseCustomSpawn", 0);
        PlayerPrefs.Save();

        // Quay về menu chính
        SceneManager.LoadScene(menuSceneName);
    }
}
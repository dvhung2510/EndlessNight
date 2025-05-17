using UnityEngine;
using UnityEngine.SceneManagement;

public class test : MonoBehaviour
{
    [Tooltip("Tên scene sẽ tải khi Player va chạm")]
    public string targetSceneName = "Map1";

    [Tooltip("Hiển thị debug log")]
    public bool showDebugLogs = true;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (showDebugLogs)
            Debug.Log("OnTriggerEnter2D: " + collision.gameObject.name);

        // Kiểm tra nếu đó là Player
        if (collision.CompareTag("Player"))
        {
            if (showDebugLogs)
                Debug.Log("Player đã được phát hiện, đang tải scene: " + targetSceneName);

            // Kiểm tra xem tên scene có được cung cấp không
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("Tên scene không được cung cấp!");
            }
        }
    }
}
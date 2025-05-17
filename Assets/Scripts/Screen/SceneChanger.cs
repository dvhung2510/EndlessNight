using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string targetSceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Trigger activated by player: " + collision.gameObject.name);
            ChangeScene();
        }
    }

    public void ChangeScene()
    {
        Debug.Log("Changing to scene: " + targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }
}
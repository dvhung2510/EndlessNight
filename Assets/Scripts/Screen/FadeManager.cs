using UnityEngine;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;
    public CanvasGroup fadePanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 0;
            fadePanel.blocksRaycasts = false;
        }
    }

    public IEnumerator FadeIn(float fadeTime)
    {
        if (fadePanel == null) yield break;

        fadePanel.blocksRaycasts = true;
        float elapsedTime = 0;

        while (elapsedTime < fadeTime)
        {
            fadePanel.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    public IEnumerator FadeOut(float fadeTime)
    {
        if (fadePanel == null) yield break;

        float elapsedTime = 0;

        while (elapsedTime < fadeTime)
        {
            fadePanel.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }
}
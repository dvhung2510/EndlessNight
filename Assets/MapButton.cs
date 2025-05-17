using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MapButton : MonoBehaviour
{
    public TextMeshProUGUI mapNameText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI chestText;
    public int levelIndex;
    public string SceneName;    
    public void AutoCreateLevelSelection(int mapIndex)
    {
        this.levelIndex = mapIndex;
        mapNameText.text = "Map " + levelIndex;
        SceneName = "Map" + levelIndex;
        coinText.text = "0/" + GameProgress.instance.requiredCoins[mapIndex - 1];

    }

    public void OnClick()
    {
        // Chuyển đến scene tương ứng với map
        if(!GameProgress.instance.IsMapUnlocked(levelIndex))
        {
            return;
        }

        SceneManager.LoadScene(SceneName);
    }
}

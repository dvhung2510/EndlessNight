using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int chestCount = 0;
    public TextMeshProUGUI chestText;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        chestText.text = ": " + chestCount.ToString();
    }
}

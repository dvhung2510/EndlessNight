using UnityEngine;

public class CollectItems : MonoBehaviour
{
    public enum CollectibleType
    {
        Coin,
        Chest
    }

    public CollectibleType type;
    private int currentMapIndex;

    private void Start()
    {
        // Lấy thông tin map hiện tại từ tên scene
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentSceneName.StartsWith("Map"))
        {
            string mapIndexStr = currentSceneName.Substring(3);
            if (int.TryParse(mapIndexStr, out int mapIndex))
            {
                currentMapIndex = mapIndex;
                Debug.Log("CollectItems: Đang ở Map " + currentMapIndex);
            }
            else
            {
                Debug.LogError("Không thể chuyển đổi tên map thành số: " + mapIndexStr);
            }
        }
        else
        {
            Debug.LogWarning("Tên scene không bắt đầu bằng 'Map': " + currentSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player đã va chạm với " + (type == CollectibleType.Coin ? "coin" : "chest"));

            if (GameProgress.instance == null)
            {
                Debug.LogError("GameProgress.instance không tồn tại!");
                return;
            }

            if (type == CollectibleType.Coin)
            {
                GameProgress.instance.AddCoin(currentMapIndex);
                Debug.Log("Đã thu thập coin ở Map " + currentMapIndex);
            }
            else if (type == CollectibleType.Chest)
            {
                GameProgress.instance.AddChest(currentMapIndex);
                Debug.Log("Đã thu thập chest ở Map " + currentMapIndex);
            }

            // Hủy đối tượng sau khi thu thập
            Destroy(gameObject);
        }
    }
}
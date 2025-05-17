using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public float interactionRadius = 2f; // Khoảng cách tương tác
    public GameObject interactionPrompt; // Hộp thoại/icon thông báo
    public NPC npcDialogue; // Tham chiếu tới script đối thoại NPC

    private Transform player; // Vị trí của người chơi

    void Start()
    {
        // Tìm đối tượng người chơi
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Ẩn hộp thoại ban đầu
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void Update()
    {
        // Kiểm tra khoảng cách
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            // Hiển thị/ẩn hộp thoại dựa trên khoảng cách
            if (interactionPrompt != null)
                interactionPrompt.SetActive(distance <= interactionRadius);

            // Kiểm tra tương tác khi ở gần và nhấn phím
            if (distance <= interactionRadius && Input.GetKeyDown(KeyCode.E))
            {
                // Gọi phương thức tương tác của NPC
                if (npcDialogue != null)
                    npcDialogue.Interact();
            }
        }
    }

    // Vẽ vùng tương tác để debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
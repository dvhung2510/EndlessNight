using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.F1;
    public float interactRadius = 2f;

    // Trong script PlayerInteraction
    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("Phím tương tác được nhấn");

            // Tìm tất cả collider trong phạm vi
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius);

            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject)
                {
                    // Thử tìm component NPC trực tiếp
                    NPC npc = collider.GetComponent<NPC>();
                    if (npc != null)
                    {
                        Debug.Log("Tìm thấy NPC: " + collider.name);
                        npc.Interact();
                        return;
                    }
                }
            }

            Debug.Log("Không tìm thấy NPC trong phạm vi");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
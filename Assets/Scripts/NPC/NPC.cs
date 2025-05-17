using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Button closeButton; // Nút Close

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool isDialogueFinished = false; // Biến mới để kiểm tra đã hiển thị hết hội thoại chưa

    void Start()
    {
        // Đảm bảo panel bắt đầu ẩn
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Thiết lập sự kiện cho nút Close
        if (closeButton != null)
            closeButton.onClick.AddListener(EndDialogue);
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        Debug.Log("NPC Interact được gọi!");

        if (dialogueData == null)
        {
            Debug.LogError("Không có dữ liệu đối thoại!");
            return;
        }

        if (isDialogueActive)
        {
            // Nếu đang typing, hiển thị đầy đủ dòng hiện tại
            // Nếu không, chuyển tới dòng tiếp theo
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = dialogueData.dialogueLines[dialogueIndex];
                isTyping = false;

                // Nếu đây là dòng cuối, đánh dấu hội thoại đã kết thúc
                if (dialogueIndex == dialogueData.dialogueLines.Length - 1)
                {
                    isDialogueFinished = true;
                }
                else
                {
                    // Tự động chuyển sang dòng tiếp theo sau 3 giây
                    StartCoroutine(WaitAndNextLine(3f));
                }
            }
            else if (!isDialogueFinished)
            {
                // Chỉ chuyển sang dòng tiếp theo nếu chưa hiển thị hết
                NextLine();
            }
            // Nếu đã hiển thị hết và nhấn tương tác, không làm gì cả hoặc có thể thêm chức năng khác
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        isDialogueFinished = false;
        dialogueIndex = 0;
        dialoguePanel.SetActive(true);
        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;

        // Bắt đầu hiển thị dòng đầu tiên
        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        // Tăng index và kiểm tra xem còn dòng nào không
        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            // Đánh dấu đã hiển thị hết hội thoại, nhưng KHÔNG đóng hộp thoại
            isDialogueFinished = true;
            Debug.Log("Đã hiển thị hết tất cả hội thoại, đợi người chơi đóng");
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Tốc độ typing
        }

        isTyping = false;

        // Nếu là dòng cuối cùng, đánh dấu hội thoại đã kết thúc
        if (dialogueIndex == dialogueData.dialogueLines.Length - 1)
        {
            isDialogueFinished = true;
            Debug.Log("Đã hiển thị dòng cuối cùng, đợi người chơi đóng");
        }
        else
        {
            // Tự động chuyển sang dòng tiếp theo sau 3 giây
            StartCoroutine(WaitAndNextLine(3f));
        }
    }

    IEnumerator WaitAndNextLine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Nếu vẫn đang trong hội thoại và chưa hiển thị hết
        if (isDialogueActive && !isDialogueFinished && !isTyping)
        {
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        isDialogueFinished = false;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // Đảm bảo gỡ bỏ listener khi hủy đối tượng
        if (closeButton != null)
            closeButton.onClick.RemoveListener(EndDialogue);
    }
}
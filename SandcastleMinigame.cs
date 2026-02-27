using UnityEngine;

public class SandcastleMinigame : MonoBehaviour
{
    [Header("把 UI 和沙堡拖曳到這裡")]
    public GameObject minigameUI;        // 小遊戲的 UI 畫面 (Canvas)
    public GameObject finishedSandcastle; // 完成後的布丁沙堡

    private bool isPlayerInRange = false; // 玩家是否在沙坑內
    private bool isPlaying = false;       // 是否正在玩小遊戲
    private int clickCount = 0;           // 目前點擊次數

    void Update()
    {
        // 1. 如果玩家在範圍內，還沒開始玩，且按下鍵盤 E 鍵
        if (isPlayerInRange && !isPlaying && Input.GetKeyDown(KeyCode.E))
        {
            minigameUI.SetActive(true); // 顯示小遊戲 UI
            isPlaying = true;           // 進入遊玩狀態
            clickCount = 0;             // 重置點擊次數
            Debug.Log("開始堆沙堡！請在畫面上連點滑鼠左鍵！");
        }

        // 2. 如果正在玩遊戲，偵測滑鼠左鍵點擊 (0 代表左鍵)
        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            clickCount++; // 點擊次數 +1
            Debug.Log("堆沙子！目前進度：" + clickCount + " / 5");

            // 3. 如果點擊達 5 次，就完成遊戲
            if (clickCount >= 5)
            {
                FinishMinigame();
            }
        }
    }

    // 當玩家 (帶有 Player 標籤) 走進沙坑的感應區
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("進入沙坑，按 E 鍵開始堆沙堡");
        }
    }

    // 當玩家離開沙坑的感應區
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            isPlaying = false;
            minigameUI.SetActive(false); // 離開時自動隱藏 UI
        }
    }

    // 完成小遊戲的處理
    private void FinishMinigame()
    {
        isPlaying = false;                  // 結束遊玩狀態
        minigameUI.SetActive(false);        // 隱藏小遊戲 UI
        finishedSandcastle.SetActive(true); // 顯示布丁沙堡！
        Debug.Log("沙堡完成啦！");
    }
}
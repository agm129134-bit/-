using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 🌟 引入 Unity 最新版 Input System 的魔法書

public class PuzzleMinigame : MonoBehaviour
{
    [Header("過關獎勵設定")]
    public Sprite rewardPhoto; 
    public int maxRewards = 2; 
    private int currentRewardCount = 0; 
    private ItemBarManager itemManager;

    [Header("UI 與物件設定")]
    public GameObject minigameUI;     
    public Canvas mainCanvas;         
    
    [Tooltip("【已升級為自動抓取】如果在場景中有預設玩家可先拖入，若無則會自動尋找")]
    public Transform playerTransform; 
    
    [Header("🌐 多人連線玩家設定")]
    [Tooltip("請輸入負責連線的組員設定給『本機玩家』的標籤 (通常是 Player 或 LocalPlayer)")]
    public string localPlayerTag = "Player";

    [Header("大魚警告設定")]
    public Image whiteFrameUI;        
    public Transform bigFishTransform;
    public float dangerDistance = 5f; 

    [Header("音效設定")]
    public AudioSource audioSource;
    public AudioClip pieceLockSound;  
    public AudioClip gameClearSound;  

    [Header("拼圖進度追蹤")]
    public PuzzlePiece[] allPieces;   
    private int lockedPiecesCount = 0;

    [Header("安全生成範圍設定 (不超出邊界)")]
    public Vector2 spawnRangeX = new Vector2(-250f, 250f); 
    public Vector2 spawnRangeY = new Vector2(-350f, -200f);

    [Header("觸發點設定")]
    [Tooltip("請輸入照片預製體的名稱 (加上 (Clone) 代表生出來的)")]
    public string photoTriggerName = "Map_Puzzle_Photo_Trigger(Clone)";
    [Tooltip("玩家要多靠近照片才能按 F？")]
    public float interactDistance = 2.5f;

    private bool isPlaying = false;

    void Start()
    {
        itemManager = FindAnyObjectByType<ItemBarManager>();
    }

    void Update()
    {
        // ==========================================
        // 🌟 動態抓取本機玩家 (解決連線生成失聯問題)
        // ==========================================
        if (playerTransform == null)
        {
            GameObject localPlayer = GameObject.FindGameObjectWithTag(localPlayerTag);
            if (localPlayer != null)
            {
                playerTransform = localPlayer.transform;
            }
        }

        // ==========================================
        // 🌟 核心魔法：升級為 New Input System 的 F 鍵觸發！
        // ==========================================
        bool isFPressed = Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;

        if (!isPlaying && isFPressed)
        {
            // 🛑 【第一道鎖：檢查紅綠燈】
            if (PickableItem.lastInteractFrame == Time.frameCount) return;

            // 智慧尋找相片：如果找不到帶 (Clone) 的，就找原名
            GameObject spawnedPhoto = GameObject.Find(photoTriggerName);
            if (spawnedPhoto == null)
            {
                spawnedPhoto = GameObject.Find(photoTriggerName.Replace("(Clone)", ""));
            }
            
            if (spawnedPhoto != null && playerTransform != null)
            {
                float distance = Vector2.Distance(playerTransform.position, spawnedPhoto.transform.position);
                
                if (distance <= interactDistance)
                {
                    // 🌟 【第二道鎖：搶下紅綠燈】
                    PickableItem.lastInteractFrame = Time.frameCount;
                    StartNewGame();
                }
            }
            else
            {
                if (spawnedPhoto == null) Debug.LogWarning("⚠️ 找不到相片觸發點，請確認名稱是否正確！");
                if (playerTransform == null) Debug.LogWarning("⚠️ 找不到玩家，請確認 LocalPlayer 標籤是否正確！");
            }
        }

        // 大魚紅框警告邏輯
        if (isPlaying && bigFishTransform != null && playerTransform != null && whiteFrameUI != null)
        {
            float distance = Vector2.Distance(playerTransform.position, bigFishTransform.position);
            whiteFrameUI.color = distance <= dangerDistance ? Color.red : Color.white;
        }
    }

    private void StartNewGame()
    {
        minigameUI.SetActive(true);
        isPlaying = true;
        lockedPiecesCount = 0; 

        if (whiteFrameUI != null) whiteFrameUI.color = Color.white;

        ShuffleAndResetPieces(); 
    }

    public void CloseAndResetMinigame()
    {
        isPlaying = false;
        minigameUI.SetActive(false);
        lockedPiecesCount = 0;
        ShuffleAndResetPieces(); 
    }

    private void ShuffleAndResetPieces()
    {
        for (int i = 0; i < allPieces.Length; i++)
        {
            if (allPieces[i] != null)
            {
                float randomX = Random.Range(spawnRangeX.x, spawnRangeX.y);
                float randomY = Random.Range(spawnRangeY.x, spawnRangeY.y);
                allPieces[i].ResetPiece(new Vector2(randomX, randomY));
            }
        }
    }

    public void PieceLocked()
    {
        PlaySound(pieceLockSound); 
        lockedPiecesCount++;

        if (lockedPiecesCount >= allPieces.Length)
        {
            FinishMinigame();
        }
    }

    private void FinishMinigame()
    {
        isPlaying = false;
        PlaySound(gameClearSound);

        if (itemManager != null && currentRewardCount < maxRewards && rewardPhoto != null)
        {
            bool isSuccess = itemManager.AddItem(rewardPhoto);
            if (isSuccess)
            {
                currentRewardCount++; 
                Debug.Log($"拼圖完成！成功獲得相片道具！(目前已領取 {currentRewardCount} / {maxRewards} 次)");
                
                // ==========================================
                // 🌟 【新增】任務清單連動！
                // ==========================================
                // 假設「拆紙箱/修照片」是第 0 個任務 (Element 0)
                if (TaskListManager.Instance != null)
                {
                    TaskListManager.Instance.CompleteTask(0);
                    Debug.Log("📝 已通知任務管家畫上刪除線！");
                }
            }
        }
        else if (currentRewardCount >= maxRewards)
        {
            Debug.Log("拼圖完成！但獎勵次數已經用完了喔！");
        }

        StartCoroutine(CloseAfterDelay()); 
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(1.5f); 
        minigameUI.SetActive(false);

        if (currentRewardCount >= maxRewards)
        {
            GameObject spawnedPhoto = GameObject.Find(photoTriggerName);
            if (spawnedPhoto == null) spawnedPhoto = GameObject.Find(photoTriggerName.Replace("(Clone)", ""));

            if (spawnedPhoto != null)
            {
                Destroy(spawnedPhoto); // ⚠️ 備註：如果是連線生成的共用物件，未來需改為連線刪除語法
                Debug.Log("獎勵已拿滿，地圖上的小遊戲觸發點消失！");
            }
        }
        else
        {
            CloseAndResetMinigame();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }
}
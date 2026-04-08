using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public Transform playerTransform; 
    
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
        // 🌟 核心魔法：防衝突的 F 鍵觸發系統
        // ==========================================
        if (!isPlaying && Input.GetKeyDown(KeyCode.F))
        {
            // 🛑 【第一道鎖：檢查紅綠燈】
            // 如果這一幀（同一個瞬間）已經有地上的道具被撿起來了，我立刻退下當作沒事！
            if (PickableItem.lastInteractFrame == Time.frameCount) return;

            GameObject spawnedPhoto = GameObject.Find(photoTriggerName);
            
            if (spawnedPhoto != null && playerTransform != null)
            {
                float distance = Vector2.Distance(playerTransform.position, spawnedPhoto.transform.position);
                
                if (distance <= interactDistance)
                {
                    // 🌟 【第二道鎖：搶下紅綠燈】
                    // 小遊戲確定要開啟了！馬上把「搶答燈」按亮，告訴同一瞬間的其他道具：「不准撿！」
                    PickableItem.lastInteractFrame = Time.frameCount;

                    StartNewGame();
                }
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
            if (spawnedPhoto != null)
            {
                Destroy(spawnedPhoto);
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
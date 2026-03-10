using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMinigame : MonoBehaviour
{
    [Header("UI 與物件設定")]
    public GameObject minigameUI;     
    public Canvas mainCanvas;         

    [Header("大魚警告設定")]
    public Image whiteFrameUI;        
    public Transform playerTransform; 
    public Transform bigFishTransform;
    public float dangerDistance = 5f; 

    [Header("音效設定")]
    public AudioSource audioSource;
    // 移除了 clickSound 變數，因為用不到了
    public AudioClip pieceLockSound;  // 拼圖卡入正確位置的音效
    public AudioClip gameClearSound;  // 遊戲通關的音效

    [Header("拼圖進度追蹤")]
    public PuzzlePiece[] allPieces;   
    private int lockedPiecesCount = 0;

    [Header("安全生成範圍設定 (不超出邊界)")]
    [Tooltip("X 軸範圍 (左邊界, 右邊界)")]
    public Vector2 spawnRangeX = new Vector2(-250f, 250f); 
    [Tooltip("Y 軸範圍 (下邊界, 上邊界)")]
    public Vector2 spawnRangeY = new Vector2(-350f, -200f);

    private bool isPlayerInRange = false;
    private bool isPlaying = false;

    void Update()
    {
        // 1. 按 F 鍵準備開啟遊戲
        if (isPlayerInRange && !isPlaying && Input.GetKeyDown(KeyCode.F))
        {
            bool amIClosest = true;
            
            float myDistance = Vector2.Distance(playerTransform.position, transform.position);
            
            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(playerTransform.position, 2f);
            
            foreach (Collider2D obj in nearbyObjects)
            {
                if (obj.isTrigger && obj.gameObject != this.gameObject)
                {
                    float otherDistance = Vector2.Distance(playerTransform.position, obj.transform.position);
                    
                    if (otherDistance < myDistance)
                    {
                        amIClosest = false;
                        break;
                    }
                }
            }

            if (amIClosest)
            {
                StartNewGame();
            }
        }

        // 2. 大魚紅框警告邏輯
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
                
                Vector2 newRandomPos = new Vector2(randomX, randomY);
                
                allPieces[i].ResetPiece(newRandomPos);
            }
        }
    }

    // ==========================================
    // 🎵 音效觸發區：只有拼圖卡上去時才會呼叫這裡
    // ==========================================
    public void PieceLocked()
    {
        PlaySound(pieceLockSound); // 播放卡上去的音效
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
        StartCoroutine(CloseAfterDelay()); 
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(1.5f); 
        minigameUI.SetActive(false);
    }

    // 將多餘的 PlayClickSound() 刪除

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (isPlaying) CloseAndResetMinigame(); 
        }
    }
}
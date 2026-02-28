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
    public AudioClip clickSound;      
    public AudioClip pieceLockSound;  
    public AudioClip gameClearSound;  

    [Header("拼圖進度追蹤")]
    public PuzzlePiece[] allPieces;   
    private int lockedPiecesCount = 0;

    // 【全新魔法】直接設定一個絕對安全的隨機生成範圍！
    [Header("安全生成範圍設定 (不超出邊界)")]
    [Tooltip("X 軸範圍 (左邊界, 右邊界)")]
    public Vector2 spawnRangeX = new Vector2(-250f, 250f); 
    [Tooltip("Y 軸範圍 (下邊界, 上邊界)")]
    public Vector2 spawnRangeY = new Vector2(-350f, -200f);

    private bool isPlayerInRange = false;
    private bool isPlaying = false;

    void Update()
    {
        if (isPlayerInRange && !isPlaying && Input.GetKeyDown(KeyCode.F))
        {
            StartNewGame();
        }

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

    // ==========================================
    // 🎲 核心洗牌魔法區 (全新：範圍內隨機撒落)
    // ==========================================
    private void ShuffleAndResetPieces()
    {
        for (int i = 0; i < allPieces.Length; i++)
        {
            if (allPieces[i] != null)
            {
                // 在你設定的安全範圍內，隨機抽一個 X 和 Y 座標
                float randomX = Random.Range(spawnRangeX.x, spawnRangeX.y);
                float randomY = Random.Range(spawnRangeY.x, spawnRangeY.y);
                
                Vector2 newRandomPos = new Vector2(randomX, randomY);
                
                // 把這個新座標發給拼圖小弟
                allPieces[i].ResetPiece(newRandomPos);
            }
        }
    }
    // ==========================================

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
        StartCoroutine(CloseAfterDelay()); 
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(1.5f); 
        minigameUI.SetActive(false);
    }

    public void PlayClickSound()
    {
        PlaySound(clickSound);
    }

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
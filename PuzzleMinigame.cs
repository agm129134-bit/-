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

    private bool isPlayerInRange = false;
    private bool isPlaying = false;

    // 【新增魔法】記住所有拼圖最初的底座位置
    private Vector2[] initialPositions;

    void Start()
    {
        // 遊戲一啟動，大總管就把你排好的 6 個漂亮位子存進名單裡
        initialPositions = new Vector2[allPieces.Length];
        for (int i = 0; i < allPieces.Length; i++)
        {
            if (allPieces[i] != null)
            {
                initialPositions[i] = allPieces[i].GetComponent<RectTransform>().anchoredPosition;
            }
        }
    }

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

        // 【修改】改成呼叫洗牌重置魔法
        ShuffleAndResetPieces(); 
    }

    public void CloseAndResetMinigame()
    {
        isPlaying = false;
        minigameUI.SetActive(false);
        lockedPiecesCount = 0;

        // 關閉時也順便洗牌歸位
        ShuffleAndResetPieces(); 
    }

    // ==========================================
    // 🎲 核心洗牌魔法區 (大風吹！)
    // ==========================================
    private void ShuffleAndResetPieces()
    {
        if (initialPositions == null || initialPositions.Length == 0) return;

        // 1. 複製一份位子名單準備洗牌
        Vector2[] shuffledPositions = (Vector2[])initialPositions.Clone();

        // 2. 像洗撲克牌一樣，隨機交換位子
        for (int i = 0; i < shuffledPositions.Length; i++)
        {
            int randomIndex = Random.Range(0, shuffledPositions.Length);
            // 讓目前的位子跟隨機抽到的位子互換
            Vector2 temp = shuffledPositions[i];
            shuffledPositions[i] = shuffledPositions[randomIndex];
            shuffledPositions[randomIndex] = temp;
        }

        // 3. 把洗好的新位子發給每一塊拼圖！
        for (int i = 0; i < allPieces.Length; i++)
        {
            if (allPieces[i] != null)
            {
                allPieces[i].ResetPiece(shuffledPositions[i]);
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
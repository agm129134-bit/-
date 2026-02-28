using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMinigame : MonoBehaviour
{
    [Header("UI 與物件設定")]
    public GameObject minigameUI;     
    public Canvas mainCanvas;         // 遊戲的主要 Canvas (拖曳比例必備)

    [Header("大魚警告設定")]
    public Image whiteFrameUI;        
    public Transform playerTransform; 
    public Transform bigFishTransform;
    public float dangerDistance = 5f; 

    [Header("音效設定")]
    public AudioSource audioSource;
    public AudioClip clickSound;      // 拿起拼圖的聲音
    public AudioClip pieceLockSound;  // 拼圖吸附的喀啦聲
    public AudioClip gameClearSound;  // 全部通關的聲音

    [Header("拼圖進度追蹤")]
    public PuzzlePiece[] allPieces;   // 把 5 塊拼圖放進這裡
    private int lockedPiecesCount = 0;

    private bool isPlayerInRange = false;
    private bool isPlaying = false;

    void Update()
    {
        if (isPlayerInRange && !isPlaying && Input.GetKeyDown(KeyCode.F))
        {
            StartNewGame();
        }

        // 大魚紅框警告系統
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
        lockedPiecesCount = 0; // 進度歸零

        if (whiteFrameUI != null) whiteFrameUI.color = Color.white;

        // 呼叫所有拼圖小弟，全部給我回到畫面底部！
        foreach (var piece in allPieces)
        {
            if (piece != null) piece.ResetPiece();
        }
    }

    // 給 X 按鈕用的關閉功能
    public void CloseAndResetMinigame()
    {
        isPlaying = false;
        minigameUI.SetActive(false);
        lockedPiecesCount = 0;

        foreach (var piece in allPieces)
        {
            if (piece != null) piece.ResetPiece();
        }
    }

    // 這個是被小拼圖呼叫的：只要有一塊吸附成功，就會執行這裡
    public void PieceLocked()
    {
        PlaySound(pieceLockSound);
        lockedPiecesCount++;

        // 檢查是不是 5 塊都拼完了？
        if (lockedPiecesCount >= allPieces.Length)
        {
            FinishMinigame();
        }
    }

    private void FinishMinigame()
    {
        isPlaying = false;
        PlaySound(gameClearSound);
        Debug.Log("拼圖全部完成啦！");
        StartCoroutine(CloseAfterDelay()); 
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(1.5f); // 讓玩家欣賞 1.5 秒完成的圖，再自動關閉
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
            if (isPlaying) CloseAndResetMinigame(); // 玩家走遠強制關閉重置
        }
    }
}
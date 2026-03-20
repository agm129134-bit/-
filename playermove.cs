using System.Collections; // 為了使用協程，一定要加這行！
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 移動速度，可以在 Unity 編輯器裡隨時調整
    public float moveSpeed = 5f; 

    // ==========================================
    // 🌟 【新增魔法 1】加速相關變數
    // ------------------------------------------
    private float originalSpeed; // 用來記錄玩家原本的速度 (例如 5f)
    private Coroutine boostCoroutine; // 用來記錄目前的加速協程，避免玩家重複狂點導致Bug
    // ==========================================

    // ==========================================
    // 🌟 【新增魔法 2】初始化
    // ------------------------------------------
    void Start()
    {
        // 遊戲一開始，先把玩家目前的速度記錄下來
        originalSpeed = moveSpeed;
    }
    // ==========================================

    void Update()
    {
        // --- 這裡是你原本寫的移動邏輯，完全不動它！ ---
        
        // 1. 取得玩家輸入
        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveY = Input.GetAxisRaw("Vertical");   

        // 2. 將輸入轉換為方向向量
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // 3. 實際移動物件
        // 乘以 Time.deltaTime 是為了確保速度在不同電腦上保持一致
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }

    // ==========================================
    // 🌟 【新增魔法 3】功能按鈕：觸發加速效果！
    // 這是給 ItemBarManager 打電話過來呼叫的入口。
    // ==========================================
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        Debug.Log($"👟 玩家移動腳本：收到加速指令！準備加速 {multiplier} 倍，持續 {duration} 秒！");
        
        // 防呆機制：如果玩家本來就已經在加速了
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine); // 把舊的碼表關掉
        }

        // 啟動新的加速碼表
        boostCoroutine = StartCoroutine(BoostRoutine(multiplier, duration));
    }

    // ==========================================
    // 🌟 【新增魔法 4】加速協程 (碼表邏輯)
    // 這是一個獨立運作的碼表，時間到了自動幫玩家把速度調回來。
    // ==========================================
    IEnumerator BoostRoutine(float multiplier, float duration)
    {
        // 1. 實際加速！把玩家的速度加上倍率
        moveSpeed = originalSpeed * multiplier;
        Debug.Log($"<color=green><b>✨【加速中】目前的移動速度變為: {moveSpeed}</b></color>");

        // 2. 碼表開始計時，等待持續時間 (duration) 秒
        yield return new WaitForSeconds(duration);

        // 3. 時間到了！把玩家的速度調回原本的樣子
        moveSpeed = originalSpeed;
        Debug.Log($"<color=white><b>🔙【加速結束】速度恢復為: {moveSpeed}</b></color>");

        // 碼表歸零
        boostCoroutine = null;
    }
    // ==========================================
    
}
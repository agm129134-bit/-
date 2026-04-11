using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 🌟 單例模式 (Singleton)：讓其他程式可以隨時隨地呼叫它
    public static GameManager Instance { get; private set; }

    [Header("👥 玩家資料設定")]
    public int playerCount = 4;       // 總玩家人數 (依據你之前的截圖，幫你預設改回 4 人)
    public int maxLivesPerPlayer = 3; // 每個人有幾條命
    
    // 記帳本：公開這些陣列，讓之後的結算 UI 可以來讀取資料
    public int[] playerScores;        // 記錄 1P, 2P, 3P, 4P 撿到的垃圾數量
    public int[] playerLives;         // 記錄 1P, 2P, 3P, 4P 剩餘的生命

    [Header("⏳ 遊戲進度與條件")]
    public float gameTime = 300f;     // 遊戲時間 (例如 300 秒 = 5 分鐘)
    public int totalTrashOnMap = 50;  // 勝利條件：總共需要撿多少垃圾
    public int remainingTrash;        // 畫面上還剩下多少垃圾

    // ==========================================
    // 🌟 任務進度追蹤 (給結算畫面的失敗文字用的)
    // ==========================================
    [Header("🎯 任務進度設定")]
    public int totalTasks = 5;       // 地圖上總共有幾個任務
    public int completedTasks = 0;   // 目前已經完成了幾個

    // ==========================================
    // 🌟 【新增】垃圾分類專屬進度
    // ==========================================
    [Header("🗑️ 垃圾分類任務設定")]
    public int requiredTrashBags = 5; // 需要分完幾個垃圾袋才算完成大任務
    public int sortedTrashBags = 0;   // 目前分完了幾個

    public bool isGameOver { get; private set; } = false;

    void Awake()
    {
        // 確保整個場景只有一個 GameManager
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 遊戲一開始，初始化大家的記帳本
        playerScores = new int[playerCount];
        playerLives = new int[playerCount];
        
        for (int i = 0; i < playerCount; i++)
        {
            playerScores[i] = 0;                  // 分數歸零
            playerLives[i] = maxLivesPerPlayer;   // 滿血復活
        }
        
        remainingTrash = totalTrashOnMap;
        completedTasks = 0; // 遊戲一開始，任務完成數歸零
        sortedTrashBags = 0; // 🌟 遊戲開始時，垃圾袋進度歸零
    }

    void Update()
    {
        if (isGameOver) return;

        // ⏳ 失敗條件 1：時間倒數
        if (gameTime > 0)
        {
            gameTime -= Time.deltaTime;
            if (gameTime <= 0)
            {
                gameTime = 0;
                TriggerGameOver(false, "時間到了！清湖失敗...");
            }
        }
    }

    // ==========================================
    // 🌟 【新增】當分完「一個」垃圾袋時呼叫！
    // ==========================================
    public void OnTrashBagSorted()
    {
        if (isGameOver) return;

        sortedTrashBags++;
        Debug.Log($"垃圾袋進度：{sortedTrashBags} / {requiredTrashBags}");

        // 檢查是不是 5 個垃圾袋都分完了？
        if (sortedTrashBags >= requiredTrashBags)
        {
            // 集滿 5 個垃圾袋，才真正打卡完成「1 個大任務」！
            CompleteOneTask();
            Debug.Log("🎉 垃圾分類任務全部完成！");
        }
    }

    // ==========================================
    // 🌟 當小遊戲(堆沙堡/集滿垃圾袋)過關時，呼叫這個函數打卡！
    // ==========================================
    public void CompleteOneTask()
    {
        if (isGameOver) return;
        
        completedTasks++;
        Debug.Log($"任務完成打卡！目前進度：{completedTasks} / {totalTasks}");
    }

    // ==========================================
    // 🗑️ 玩家撿起垃圾時呼叫這個函數
    // 備註：playerId 傳入 0 代表 1P，傳入 1 代表 2P，依此類推
    // ==========================================
    public void AddScore(int playerId, int amount = 1)
    {
        if (isGameOver) return;
        if (playerId < 0 || playerId >= playerCount) return;

        playerScores[playerId] += amount; // 幫該玩家加分
        remainingTrash -= amount;         // 地圖上的垃圾減少

        // 🎉 勝利條件：垃圾全部撿完了！
        if (remainingTrash <= 0)
        {
            remainingTrash = 0;
            TriggerGameOver(true, "清湖成功！大豐收！");
        }
    }

    // ==========================================
    // 🐟 玩家被大魚咬到時呼叫這個函數
    // ==========================================
    public void TakeDamage(int playerId)
    {
        if (isGameOver) return;
        if (playerId < 0 || playerId >= playerCount) return;

        if (playerLives[playerId] > 0)
        {
            playerLives[playerId]--; // 扣該玩家一條命
            Debug.Log($"玩家 {playerId + 1} 被咬了！剩下 {playerLives[playerId]} 條命。");
        }

        // 💀 失敗條件 2：檢查是不是大家都沒命了？
        bool isEveryoneDead = true;
        for (int i = 0; i < playerCount; i++)
        {
            if (playerLives[i] > 0)
            {
                isEveryoneDead = false;
                break;
            }
        }

        if (isEveryoneDead)
        {
            TriggerGameOver(false, "所有玩家都陣亡了！驚！大魚來襲！");
        }
    }

    // ==========================================
    // 🛑 遊戲結束判定總機
    // ==========================================
    private void TriggerGameOver(bool isWin, string reason)
    {
        isGameOver = true;
        Debug.Log($"<color=orange>【遊戲結束】狀態：{(isWin ? "成功" : "失敗")} | 原因：{reason}</color>");

        // ==========================================
        // 🌟 確保遊戲結束時，自動呼叫結算畫面！
        // ==========================================
        if (SummaryUIManager.Instance != null)
        {
            SummaryUIManager.Instance.ShowSummary(isWin);
        }
    }
}
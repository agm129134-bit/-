using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 🌟 單例模式 (Singleton)：讓其他程式可以隨時隨地呼叫它
    public static GameManager Instance { get; private set; }

    [Header("👥 玩家資料設定")]
    [Tooltip("⚠️ 單人測試請務必改成 1！不然大魚咬死你之後，系統會以為還有其他玩家活著！")]
    public int playerCount = 4;       // 總玩家人數 (單人測試時請在 Inspector 改成 1)
    public int maxLivesPerPlayer = 3; // 每個人有幾條命
    
    // 記帳本：公開這些陣列，讓之後的結算 UI 可以來讀取資料
    public int[] playerScores;        // 記錄 1P, 2P, 3P, 4P 撿到的垃圾數量
    public int[] playerLives;         // 記錄 1P, 2P, 3P, 4P 剩餘的生命

    // ==========================================
    // 🏆 動態雙重勝利條件
    // ==========================================
    [Header("🏆 勝利條件設定")]
    [Tooltip("過關總共需要撿到幾個垃圾？ (例如設定 60)")]
    public int targetTrashCount = 60; 
    [Tooltip("每個『存活』的玩家需要完成幾個小遊戲？ (例如設定 3)")]
    public int tasksPerPlayer = 3;

    [Header("📊 當前進度 (自動計算，不用自己填)")]
    public int currentCollectedTrash = 0; // 目前已經撿到的垃圾總數
    public int alivePlayerCount;          // 目前存活的玩家數量

    // 💡 魔法屬性：會根據活著的人數自動算出目標！
    public int CurrentTargetTasks 
    { 
        get { return alivePlayerCount * tasksPerPlayer; } 
    }

    [Header("⏳ 遊戲進度與條件 (相容舊UI)")]
    public int totalTrashOnMap = 60;  // 為了配合舊功能保留
    public int remainingTrash;        // 畫面上還剩下多少垃圾 (失敗畫面可能會用到)

    [Header("🎯 任務進度設定")]
    public int totalTasks = 12;      // UI顯示用的總數 (可留著給其他腳本參考)
    public int completedTasks = 0;   // 目前已經完成了幾個

    public bool isGameOver { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        playerScores = new int[playerCount];
        playerLives = new int[playerCount];
        
        for (int i = 0; i < playerCount; i++)
        {
            playerScores[i] = 0;                  // 分數歸零
            playerLives[i] = maxLivesPerPlayer;   // 滿血復活
        }
        
        alivePlayerCount = playerCount; // 遊戲一開始，大家都活著
        currentCollectedTrash = 0;
        remainingTrash = totalTrashOnMap;
        completedTasks = 0; 
    }

    // ==========================================
    // 🌟 現在垃圾分類成功，直接算作「收集到一般垃圾」！
    // 加入 playerId 參數 (預設為 0 代表 1P)，方便你把分數算給對應的玩家
    // ==========================================
    public void OnTrashBagSorted(int playerId = 0)
    {
        if (isGameOver) return;

        Debug.Log("🗑️ 透過分類小遊戲成功處理了垃圾！");
        
        // 直接呼叫 AddScore，把它當作一般垃圾加分！(預設加 1 個)
        AddScore(playerId, 1); 
    }

    // ==========================================
    // 🌟 當小遊戲(堆沙堡等)過關時，呼叫這個函數打卡！
    // ==========================================
    public void CompleteOneTask()
    {
        if (isGameOver) return;
        
        completedTasks++;
        Debug.Log($"任務完成打卡！目前任務進度：{completedTasks} / {CurrentTargetTasks}");
        
        CheckWinCondition();
    }

    // ==========================================
    // 🗑️ 玩家撿起垃圾(或分類成功)時呼叫這個函數
    // ==========================================
    public void AddScore(int playerId, int amount = 1)
    {
        if (isGameOver) return;
        if (playerId < 0 || playerId >= playerCount) return;

        playerScores[playerId] += amount; // 幫該玩家加分 (結算畫面「今日最佳」會用到)
        currentCollectedTrash += amount;  // 總垃圾收集量增加
        remainingTrash -= amount;         // 地圖上的垃圾減少 (相容舊UI)

        Debug.Log($"撿到垃圾！目前垃圾進度：{currentCollectedTrash} / {targetTrashCount}");

        CheckWinCondition();
    }

    // ==========================================
    // 🌟 中央大腦判斷勝利條件
    // ==========================================
    private void CheckWinCondition()
    {
        if (isGameOver) return;

        // 雙重門檻：垃圾達標 且 任務達標
        if (currentCollectedTrash >= targetTrashCount && completedTasks >= CurrentTargetTasks)
        {
            TriggerGameOver(true, $"清湖成功！大豐收！\n垃圾: {currentCollectedTrash}/{targetTrashCount} | 任務: {completedTasks}/{CurrentTargetTasks}");
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
            playerLives[playerId]--; 
            Debug.Log($"玩家 {playerId + 1} 被咬了！剩下 {playerLives[playerId]} 條命。");

            // 通知血量 UI 管家更新畫面！(把紅心變灰心)
            if (HealthUIManager.Instance != null)
            {
                HealthUIManager.Instance.UpdateHealth(playerId, playerLives[playerId]);
            }

            // 🛑 如果被咬完這口，生命值歸零，代表死掉了
            if (playerLives[playerId] == 0)
            {
                alivePlayerCount--; 
                Debug.Log($"💀 玩家 {playerId + 1} 陣亡！目前剩餘存活人數：{alivePlayerCount}。目標任務數降為：{CurrentTargetTasks}");

                // 🌟 【新增】完美連動任務清單管家：在死掉的玩家頭像上畫紅叉叉！
                if (TaskListManager.Instance != null)
                {
                    TaskListManager.Instance.MarkPlayerDead(playerId);
                }
            }
        }

        if (alivePlayerCount <= 0)
        {
            TriggerGameOver(false, "所有玩家都陣亡了！驚！大魚來襲！");
        }
        else
        {
            CheckWinCondition();
        }
    }

    // ==========================================
    // 🛑 遊戲結束判定總機
    // ==========================================
    public void TriggerGameOver(bool isWin, string reason)
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log($"<color=orange>【遊戲結束】狀態：{(isWin ? "成功" : "失敗")} | 原因：{reason}</color>");

        if (SummaryUIManager.Instance != null)
        {
            SummaryUIManager.Instance.ShowSummary(isWin);
        }
        else
        {
            Debug.LogWarning("⚠️ 找不到 SummaryUIManager！請確認它有在場景中開啟！");
        }
    }
}
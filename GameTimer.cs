using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    // ==========================================
    // 🌟 【新增魔法 1：單例模式】
    // 讓別人只要打 "GameTimer.Instance" 就能立刻找到我！
    // ==========================================
    public static GameTimer Instance { get; private set; }

    [Header("UI 設定")]
    [Tooltip("把畫面上方的 TimeText 拖進來")]
    public Text timerText; 

    [Header("時間設定")]
    [Tooltip("你想設定倒數幾秒？ (例如輸入 120 就是 2 分鐘)")]
    public float startingTimeInSeconds = 120f; 

    private float currentTime;
    private bool isTimerRunning = false;

    // 用來確保文字變色警告只觸發一次
    private bool isWarningTriggered = false;

    void Awake()
    {
        // 確保場景中只有一個 GameTimer 實體
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 如果跨關卡計時器不歸零，就拿掉斜線
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 遊戲一開始，把當前時間設定為你填寫的總秒數
        currentTime = startingTimeInSeconds;
        isWarningTriggered = false;
        isTimerRunning = true; 
    }

    void Update()
    {
        if (isTimerRunning)
        {
            // 時間不斷倒扣 (Time.deltaTime 是一幀經過的時間)
            currentTime -= Time.deltaTime;

            // 如果時間扣到 0 或變成負數了
            if (currentTime <= 0)
            {
                currentTime = 0; // 鎖定在 0，避免出現負數 (-00:01)
                isTimerRunning = false; // 停止計時
                UpdateTimerDisplay(); // 更新 UI 顯示為 00:00
                TimeIsUp(); // 呼叫「時間到」的處理函數
            }
            else
            {
                // 更新畫面上的數字
                UpdateTimerDisplay();
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        // 把總秒數換算成「幾分」跟「幾秒」
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        // 更新 UI 文字，格式化為 "00:00" 的樣子
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // 💡 視覺小魔法：如果時間剩下不到 10 秒，讓文字變紅色警告玩家！
            if (currentTime <= 10f && currentTime > 0 && !isWarningTriggered)
            {
                timerText.color = Color.red;
                isWarningTriggered = true; // 標記已觸發警告
            }
            // 如果時間又被加回 10 秒以上，把顏色變回白色
            else if (currentTime > 10f && isWarningTriggered)
            {
                timerText.color = Color.white;
                isWarningTriggered = false; // 重置警告標記
            }
        }
    }

    // ==========================================
    // 🌟 【新增魔法 2：功能按鈕：增加遊戲時間】
    // 別的腳本（例如道具欄）呼叫這個，就可以加時間
    // ==========================================
    public void AddGameTime(float timeToAddInSeconds)
    {
        // 如果遊戲還在跑，就幫他加秒數
        if (isTimerRunning)
        {
            currentTime += timeToAddInSeconds;
            
            // 💡 選擇性防呆：如果不想讓時間超過原本設定的起點，就把斜線拿掉
            // currentTime = Mathf.Min(currentTime, startingTimeInSeconds);
            
            Debug.Log($"吃了時間藥水！遊戲時間增加了 {timeToAddInSeconds} 秒！");
        }
    }

    // ==========================================
    // 🚨 時間到的處理區塊
    // ==========================================
    private void TimeIsUp()
    {
        Debug.Log("🚨🚨🚨 時間到！遊戲結束！🚨🚨🚨");
        
        // TODO: 這裡未來可以呼叫「遊戲失敗 (Game Over)」的畫面
        // 例如：UIManager.ShowGameOverScreen();
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class SummaryUIManager : MonoBehaviour
{
    public static SummaryUIManager Instance { get; private set; }

    // ==========================================
    // 🌟 【新增】開發測試開關
    // ==========================================
    [Header("🛠️ 開發測試設定")]
    [Tooltip("打勾：開啟 Y 鍵(勝利)與 N 鍵(失敗)的測試功能 / 取消打勾：關閉測試按鍵")]
    public bool enableTestKeys = true;

    [System.Serializable]
    public struct PlayerProfile
    {
        public string playerName;
        public Sprite playerAvatar;
    }

    [Header("👥 玩家圖文資料庫 (請依 ID 順序填寫 1P~4P)")]
    public PlayerProfile[] playerProfiles;

    [Header("🎮 遊戲進行中 UI (結算時會自動隱藏)")]
    public GameObject inGameTimerUI;      

    [Header("🛑 結算時強制關閉的視窗 (把小遊戲介面拖進來)")]
    public GameObject[] panelsToCloseOnSummary;

    [Header("🎬 階段一：5秒全螢幕提示")]
    public GameObject phase1_Panel;       
    public GameObject successBigImage;    
    public GameObject failBigImage;       
    public float showTime = 5f;           

    [Header("📰 階段二：大魚快報 (滑動面板)")]
    public RectTransform newspaperPanel;  
    public GameObject successNewsGroup;   
    public GameObject failNewsGroup;      

    [Header("🏆 成功版 UI 元件綁定")]
    public Image bestPlayerAvatar;
    public Text bestPlayerName;
    public Text bestPlayerScore;
    public Image lazyPlayerAvatar;
    public Text lazyPlayerName;
    public Text lazyPlayerScore;

    [Header("💀 失敗版 UI 元件綁定")]
    public Text remainingTrashText;
    public Text failedPlayersText;

    [Header("動畫設定")]
    public Vector2 newspaperStartPos = new Vector2(0, -1500f); 
    public Vector2 newspaperEndPos = new Vector2(0, 0);        
    public float slideDuration = 1f;                           

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (phase1_Panel != null) phase1_Panel.SetActive(false);
        if (newspaperPanel != null) newspaperPanel.anchoredPosition = newspaperStartPos;
    }

   void Update()
    {
        // ==========================================
        // 🌟 【修改】用開關包住測試按鍵邏輯
        // ==========================================
        if (enableTestKeys)
        {
            // 🛠️ 測試按鍵
            if (Input.GetKeyDown(KeyCode.Y)) 
            {
                // 拔掉塞分數的作弊碼，現在按 Y 只會單純叫出「真實成績」的勝利畫面
                ShowSummary(true); 
            }
            if (Input.GetKeyDown(KeyCode.N)) 
            {
                if (GameManager.Instance != null)
                {
                    // 這個也可以順便拔掉，讓它顯示真實剩下的垃圾數量
                    // GameManager.Instance.remainingTrash = 20; 
                }
                ShowSummary(false); 
            }
        }
    }

    public void ShowSummary(bool isWin)
    {
        Time.timeScale = 0f;
        StartCoroutine(SummarySequence(isWin));
    }

    private IEnumerator SummarySequence(bool isWin)
    {
        if (inGameTimerUI != null) inGameTimerUI.SetActive(false);

        if (panelsToCloseOnSummary != null)
        {
            foreach (GameObject panel in panelsToCloseOnSummary)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        PrepareData(isWin);

        if (phase1_Panel != null) phase1_Panel.SetActive(true);
        if (successBigImage != null) successBigImage.SetActive(isWin);
        if (failBigImage != null) failBigImage.SetActive(!isWin);
        
        yield return new WaitForSecondsRealtime(showTime);

        if (phase1_Panel != null) phase1_Panel.SetActive(false);

        if (successNewsGroup != null) successNewsGroup.SetActive(isWin);
        if (failNewsGroup != null) failNewsGroup.SetActive(!isWin);
        
        if (newspaperPanel != null)
        {
            float timer = 0f;
            while (timer < slideDuration)
            {
                timer += Time.unscaledDeltaTime; 
                
                float progress = timer / slideDuration;
                float smoothProgress = 1f - Mathf.Pow(1f - progress, 3f); 
                
                newspaperPanel.anchoredPosition = Vector2.Lerp(newspaperStartPos, newspaperEndPos, smoothProgress);
                yield return null;
            }
            
            newspaperPanel.anchoredPosition = newspaperEndPos;
        }
    }

    private void PrepareData(bool isWin)
    {
        if (GameManager.Instance == null) return;

        if (isWin)
        {
            int bestIndex = 0;
            int lazyIndex = 0;
            int maxScore = -1;
            int minScore = 9999;

            for (int i = 0; i < GameManager.Instance.playerCount; i++)
            {
                int score = GameManager.Instance.playerScores[i];
                if (score > maxScore) { maxScore = score; bestIndex = i; }
                if (score < minScore) { minScore = score; lazyIndex = i; }
            }

            if (playerProfiles.Length > bestIndex)
            {
                if (bestPlayerName != null) bestPlayerName.text = playerProfiles[bestIndex].playerName;
                if (bestPlayerAvatar != null) bestPlayerAvatar.sprite = playerProfiles[bestIndex].playerAvatar;
                if (bestPlayerScore != null) bestPlayerScore.text = $"撿了 {maxScore} 個垃圾!!";
            }
            
            if (playerProfiles.Length > lazyIndex)
            {
                if (lazyPlayerName != null) lazyPlayerName.text = playerProfiles[lazyIndex].playerName;
                if (lazyPlayerAvatar != null) lazyPlayerAvatar.sprite = playerProfiles[lazyIndex].playerAvatar;
                if (lazyPlayerScore != null) lazyPlayerScore.text = $"只撿 {minScore} 個垃圾...";
            }
        }
        else
        {
            if (remainingTrashText != null) remainingTrashText.text = GameManager.Instance.remainingTrash.ToString() + " 個";
            
            int uncompletedTasks = GameManager.Instance.totalTasks - GameManager.Instance.completedTasks;
            if (uncompletedTasks < 0) uncompletedTasks = 0; 
            
            if (failedPlayersText != null) failedPlayersText.text = $"{uncompletedTasks} 個任務未完成...";
        }
    }

    public void Button_RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void Button_MainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }
}
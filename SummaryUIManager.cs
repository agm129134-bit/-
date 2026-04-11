using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class SummaryUIManager : MonoBehaviour
{
    public static SummaryUIManager Instance { get; private set; }

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
        // 🛠️ 測試按鍵
        if (Input.GetKeyDown(KeyCode.Y)) 
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerScores[0] = 40;
                GameManager.Instance.playerScores[1] = 10;
                GameManager.Instance.playerScores[2] = 25;
            }
            ShowSummary(true); 
        }
        if (Input.GetKeyDown(KeyCode.N)) 
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.remainingTrash = 20;
                // 🛑 已經把強制設定 completedTasks = 3 的作弊程式碼刪除了！
                // 現在它會真實反映你破關的數量！
            }
            ShowSummary(false); 
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
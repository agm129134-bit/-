using UnityEngine;
using UnityEngine.UI;

// 🌟 【新增】要求這個腳本所在的物件一定要有 AudioSource 元件
[RequireComponent(typeof(AudioSource))] 
public class GameTimer : MonoBehaviour
{
    // ==========================================
    // 🌟 單例模式
    // ==========================================
    public static GameTimer Instance { get; private set; }

    [Header("UI 設定")]
    [Tooltip("把畫面上方的 TimeText 拖進來")]
    public Text timerText; 

    [Header("時間設定")]
    [Tooltip("你想設定倒數幾秒？ (例如輸入 120 就是 2 分鐘)")]
    public float startingTimeInSeconds = 120f; 

    // ==========================================
    // 🌟 倒數音效設定
    // ==========================================
    [Header("倒數音效設定")]
    [Tooltip("把你生成的『完整 10 秒滴答聲』音效檔案拖進來")]
    public AudioClip countdownSound; 
    private AudioSource audioSource;
    
    // 🌟 【修改】改成用布林值，記錄這段長音軌是不是已經開始播了
    private bool hasPlayedCountdownAudio = false; 

    private float currentTime;
    private bool isTimerRunning = false;
    private bool isWarningTriggered = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentTime = startingTimeInSeconds;
        isWarningTriggered = false;
        isTimerRunning = true; 
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0; 
                isTimerRunning = false; 
                UpdateTimerDisplay(); 
                TimeIsUp(); 
            }
            else
            {
                UpdateTimerDisplay();
                
                // 檢查是否要播放倒數音效
                CheckCountdownSound();
            }
        }
    }

    // ==========================================
    // 🌟 【修改】適應「長音軌」的播放邏輯
    // ==========================================
    private void CheckCountdownSound()
    {
        // 當時間小於等於 10 秒，而且音軌還沒開始播
        if (currentTime <= 10f && currentTime > 0) 
        {
            if (!hasPlayedCountdownAudio)
            {
                if (countdownSound != null && audioSource != null)
                {
                    audioSource.clip = countdownSound;
                    audioSource.Play(); // 直接播放整段音軌
                }
                
                // 標記為已經播放，這樣才不會每一幀都重新播
                hasPlayedCountdownAudio = true;
            }
        }
        else if (currentTime > 10f)
        {
            // 如果玩家吃了加時道具，時間大於 10 秒了
            hasPlayedCountdownAudio = false; // 重置標記
            
            // 如果倒數音效還在播，趕快卡掉
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == countdownSound)
            {
                audioSource.Stop(); 
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            if (currentTime <= 10f && currentTime > 0 && !isWarningTriggered)
            {
                timerText.color = Color.red;
                isWarningTriggered = true; 
            }
            else if (currentTime > 10f && isWarningTriggered)
            {
                timerText.color = Color.white;
                isWarningTriggered = false; 
            }
        }
    }

    public void AddGameTime(float timeToAddInSeconds)
    {
        if (isTimerRunning)
        {
            currentTime += timeToAddInSeconds;
            Debug.Log($"吃了時間藥水！遊戲時間增加了 {timeToAddInSeconds} 秒！");
        }
    }

    private void TimeIsUp()
    {
        Debug.Log("🚨🚨🚨 時間到！遊戲結束！🚨🚨🚨");
        
        // ==========================================
        // 🌟 【新增】時間到了，強制停止倒數音效
        // 避免結算畫面出來了，背景還在滴答滴答響
        // ==========================================
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        if (SummaryUIManager.Instance != null)
        {
            SummaryUIManager.Instance.ShowSummary(false); 
        }
        else
        {
            Debug.LogWarning("⚠️ 找不到 SummaryUIManager！請確認場景中有掛載結算腳本的物件。");
        }
    }
}
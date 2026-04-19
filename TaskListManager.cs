using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; 

public class TaskListManager : MonoBehaviour
{
    public static TaskListManager Instance { get; private set; }

    [Header("UI 總開關設定")]
    public GameObject taskListUI;
    public bool isOpenAtStart = false;

    [Header("👥 玩家陣亡狀態設定")]
    public GameObject[] deadCrosses;

    // ==========================================
    // 🎬 【新增】滑動動畫設定
    // ==========================================
    [Header("🎬 動畫設定")]
    [Tooltip("開關動畫需要幾秒鐘？(預設 0.4秒)")]
    public float slideDuration = 0.4f;
    [Tooltip("關閉時要往左邊退到多遠？(數值越大退越遠，預設 500)")]
    public float slideOffset = 500f;

    private RectTransform uiRectTransform;
    private Vector2 shownPosition;  // 打開時的座標 (也就是你在編輯器裡排版的原位)
    private Vector2 hiddenPosition; // 關閉時的座標 (往左移)
    private Coroutine currentSlideCoroutine;

    // ==========================================
    // 🌟 針對 Legacy Text 升級的任務結構
    // ==========================================
    [System.Serializable]
    public struct TaskUI
    {
        public string taskName;        // 備註用
        public Text taskText;          // 🌟 舊版 Text 組件
        public GameObject checkMark;   // 打勾圖片
        public GameObject strikeLine;  // 🌟 刪除線圖片 (Image)
    }

    [Header("📝 任務進度設定")]
    public TaskUI[] tasks;

    private bool isShowing = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 確保所有紅叉叉、打勾勾、刪除線一開始都是隱藏的
        foreach (var cross in deadCrosses)
        {
            if (cross != null) cross.SetActive(false);
        }

        foreach (var task in tasks)
        {
            if (task.checkMark != null) task.checkMark.SetActive(false);
            if (task.strikeLine != null) task.strikeLine.SetActive(false);
        }

        // ==========================================
        // 🌟 初始化動畫座標
        // ==========================================
        if (taskListUI != null)
        {
            // 抓取 UI 的排版元件
            uiRectTransform = taskListUI.GetComponent<RectTransform>();
            
            // 紀錄現在的位子當作「打開時的位置」
            shownPosition = uiRectTransform.anchoredPosition;
            // 算好「關閉時的位置」 (把 X 座標減掉偏移量，讓它躲到左邊畫面外)
            hiddenPosition = new Vector2(shownPosition.x - slideOffset, shownPosition.y);

            isShowing = isOpenAtStart;
            
            // 根據預設狀態，直接把它擺到對應的位置
            if (isShowing)
            {
                uiRectTransform.anchoredPosition = shownPosition;
                taskListUI.SetActive(true);
            }
            else
            {
                uiRectTransform.anchoredPosition = hiddenPosition;
                taskListUI.SetActive(false);
            }
        }
    }

    void Update()
    {
        // 偵測 TAB 鍵按下
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            isShowing = !isShowing;

            // 如果目前正在播動畫，先把它停掉，免得打開一半又按關閉會錯亂
            if (currentSlideCoroutine != null) StopCoroutine(currentSlideCoroutine);
            
            // 播放新的滑動動畫
            currentSlideCoroutine = StartCoroutine(SlideUI(isShowing));
        }
    }

    // ==========================================
    // 🎬 控制 UI 滑動的魔法協程
    // ==========================================
    private IEnumerator SlideUI(bool show)
    {
        // 如果是要打開，先讓物件顯示出來，才能看到它滑進來
        if (show) taskListUI.SetActive(true);

        Vector2 startPos = uiRectTransform.anchoredPosition;
        Vector2 targetPos = show ? shownPosition : hiddenPosition;
        float time = 0;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            
            // 讓進度在 0 ~ 1 之間
            float t = time / slideDuration;
            
            // 加上一點「減速(Ease-Out)」效果，讓滑動看起來更自然，不會死死的
            t = t * (2f - t); 

            uiRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 確保最後精準停在目標位置
        uiRectTransform.anchoredPosition = targetPos;

        // 如果是要關閉，等它徹底滑出畫面後，再把物件關掉省效能
        if (!show) taskListUI.SetActive(false);
    }

    // ==========================================
    // ✅ 任務完成：顯示打勾 + 顯示刪除線 + 文字變淡
    // ==========================================
    public void CompleteTask(int taskIndex)
    {
        if (taskIndex >= 0 && taskIndex < tasks.Length)
        {
            if (tasks[taskIndex].checkMark != null)
                tasks[taskIndex].checkMark.SetActive(true);

            if (tasks[taskIndex].strikeLine != null)
                tasks[taskIndex].strikeLine.SetActive(true);

            if (tasks[taskIndex].taskText != null)
            {
                Color fadedColor = tasks[taskIndex].taskText.color;
                fadedColor.a = 0.5f; 
                tasks[taskIndex].taskText.color = fadedColor;
            }
            
            Debug.Log($"✅ 任務 {taskIndex + 1} 已完成！畫上刪除線！");
        }
    }

    public void MarkPlayerDead(int playerId)
    {
        if (playerId >= 0 && playerId < deadCrosses.Length)
        {
            if (deadCrosses[playerId] != null) deadCrosses[playerId].SetActive(true);
        }
    }
}
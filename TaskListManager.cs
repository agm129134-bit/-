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
    // 🌟 針對 Legacy Text 升級的任務結構
    // ==========================================
    [System.Serializable]
    public struct TaskUI
    {
        public string taskName;        // 備註用
        public Text taskText;          // 🌟 舊版 Text 組件
        public GameObject checkMark;   // 打勾圖片
        public GameObject strikeLine;  // 🌟 【新增】刪除線圖片 (Image)
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

        isShowing = isOpenAtStart;
        if (taskListUI != null) taskListUI.SetActive(isShowing);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            isShowing = !isShowing;
            taskListUI.SetActive(isShowing);
        }
    }

    // ==========================================
    // ✅ 任務完成：顯示打勾 + 顯示刪除線 + 文字變淡
    // ==========================================
    public void CompleteTask(int taskIndex)
    {
        if (taskIndex >= 0 && taskIndex < tasks.Length)
        {
            // 1. 顯示打勾圖片
            if (tasks[taskIndex].checkMark != null)
                tasks[taskIndex].checkMark.SetActive(true);

            // 2. 🌟 顯示刪除線實體圖片
            if (tasks[taskIndex].strikeLine != null)
                tasks[taskIndex].strikeLine.SetActive(true);

            // 3. 🌟 讓文字顏色變淡 (透明度降為 50%)，視覺效果更好！
            if (tasks[taskIndex].taskText != null)
            {
                Color fadedColor = tasks[taskIndex].taskText.color;
                fadedColor.a = 0.5f; // a 代表 Alpha (透明度)，範圍是 0~1
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
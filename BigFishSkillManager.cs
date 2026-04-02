using UnityEngine;
using UnityEngine.UI; // 控制 UI 必加

public class BigFishSkillManager : MonoBehaviour
{
    [Header("技能設定")]
    [Tooltip("請把做好的『口水彈預製體』拖進來")]
    public GameObject spitPrefab;
    [Tooltip("口水彈從哪裡發射？(請在大魚嘴巴位置放一個空物件拖進來)")]
    public Transform firePoint;
    [Tooltip("要瞄準誰？(請把人類玩家拖進來)")]
    public Transform targetHuman;

    [Header("冷卻時間 (CD) 設定")]
    public float cooldownTime = 20f; // 冷卻 20 秒
    private float currentCooldown = 0f; // 目前剩下的冷卻時間

    [Header("UI 設定")]
    [Tooltip("用來顯示冷卻轉圈圈的半透明黑色遮罩圖片")]
    public Image cooldownMaskImage; 
    [Tooltip("技能按鈕本身 (用來在 CD 時關閉點擊)")]
    public Button skillButton;

    void Start()
    {
        // 遊戲一開始，確保技能是可以用的
        currentCooldown = 0f;
        if (cooldownMaskImage != null) cooldownMaskImage.fillAmount = 0f;
    }

    void Update()
    {
        // 🌟 處理冷卻時間的倒數
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime; // 每一幀扣除時間
            
            // 更新 UI 轉圈圈的比例 (剩餘時間 / 總時間 = 0~1 的比例)
            if (cooldownMaskImage != null)
            {
                cooldownMaskImage.fillAmount = currentCooldown / cooldownTime;
            }

            // CD 期間按鈕不能按
            if (skillButton != null) skillButton.interactable = false;
        }
        else
        {
            // CD 結束，按鈕恢復可以按的狀態
            if (skillButton != null) skillButton.interactable = true;
        }
    }

    // ==========================================
    // 🌟 給 UI 按鈕呼叫的發射函數
    // ==========================================
    public void CastSpitSkill()
    {
        // 雙重保險：確認 CD 已經轉好，且必要物件都在
        if (currentCooldown <= 0 && spitPrefab != null && firePoint != null && targetHuman != null)
        {
            Debug.Log("💦 大魚發射口水彈！");

            // 1. 計算人類相對於發射點的方向角度
            Vector2 direction = (targetHuman.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // 2. 轉動口水彈，讓口水彈的「車頭」對準人類
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            // 3. 生成口水彈實體！
            Instantiate(spitPrefab, firePoint.position, rotation);

            // 4. 技能進入冷卻！
            currentCooldown = cooldownTime;
        }
    }
}
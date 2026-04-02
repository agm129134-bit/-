using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemBarManager : MonoBehaviour
{
    [Header("UI 設定")]
    [Tooltip("把 Slot_1 到 Slot_3 裡面的 ItemIcon 拖曳進來")]
    public Image[] itemIcons; 

    // ----------------------------------------------------
    // ⏳ 加時道具設定 (沙漏/瓶子)
    // ----------------------------------------------------
    [Header("⏳ 加時道具設定 (沙漏/瓶子)")]
    public Sprite hourglassSprite; 
    public float addTimeSeconds = 30f; 

    // 🌟 【超級新增魔法：加時飄字特效】
    [Header("⏳ 加時飄字特效設定")]
    [Tooltip("請把 Project 視窗裡做好的飄字預製體拖進來")]
    public GameObject timePopupPrefab; 
    
    [Tooltip("請把畫面中央顯示『00 : 25』的那個 Text 物件拖進來 (作為飄字的參考點)")]
    public RectTransform timeTextReferencePoint; 
    // ----------------------------------------------------

    // 👟 加速道具設定 (紅鞋子)
    [Header("👟 加速道具設定 (紅鞋子)")]
    public Sprite shoeSprite; 
    public float speedBoostDuration = 5f;
    public float speedMultiplier = 1.5f;
    public GameObject playerObject; 

    private Sprite[] currentItems;
    private int currentSelectedIndex = 0; 

    void Start()
    {
        currentItems = new Sprite[itemIcons.Length];
        for (int i = 0; i < itemIcons.Length; i++) ClearSlot(i);
        SelectSlot(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SelectSlot(2);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            UseItem(currentSelectedIndex);
        }
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= itemIcons.Length) return;
        currentSelectedIndex = index;
        for (int i = 0; i < itemIcons.Length; i++)
        {
            if (itemIcons[i] != null)
            {
                Transform slotParent = itemIcons[i].transform.parent;
                if (slotParent != null)
                {
                    slotParent.localScale = i == currentSelectedIndex ? new Vector3(1.15f, 1.15f, 1f) : new Vector3(1f, 1f, 1f);
                }
            }
        }
    }

    public bool AddItem(Sprite newItemSprite)
    {
        for (int i = 0; i < currentItems.Length; i++)
        {
            if (currentItems[i] == null)
            {
                currentItems[i] = newItemSprite;      
                itemIcons[i].sprite = newItemSprite;  
                itemIcons[i].color = new Color(1, 1, 1, 1); 
                return true; 
            }
        }
        return false; 
    }

    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentItems.Length) return;

        Sprite itemToUse = currentItems[slotIndex];

        if (itemToUse != null)
        {
            // =========================
            // 判定 1：【⏳ 使用時鐘道具】
            // =========================
            if (hourglassSprite != null && itemToUse == hourglassSprite)
            {
                // 1. 執行原本的加時間邏輯
                if (GameTimer.Instance != null)
                {
                    GameTimer.Instance.AddGameTime(addTimeSeconds);
                    
                    // 🌟 2. 【核心魔法】觸發加時飄字特效！
                    TriggerTimePopupEffect(addTimeSeconds);
                }
            }
            // =========================
            // 判定 2：【👟 使用加速道具】
            // =========================
            else if (shoeSprite != null && itemToUse == shoeSprite)
            {
                if (playerObject != null)
                {
                    PlayerMovement pm = playerObject.GetComponent<PlayerMovement>();
                    if (pm != null) pm.ActivateSpeedBoost(speedMultiplier, speedBoostDuration);
                }
            }

            ClearSlot(slotIndex);
        }
    }

    // ==========================================
    // 🌟 處理加時飄字的核心函數
    // ==========================================
    private void TriggerTimePopupEffect(float amount)
    {
        // 雙重保險：確認模具跟參考點都在
        if (timePopupPrefab == null || timeTextReferencePoint == null)
        {
            Debug.LogWarning("🚨 [錯誤] ItemBarManager 忘記綁定飄字預製體或時間參考點了！特效不會顯示喔。");
            return;
        }

        // 1. 計算生成位置：找到畫面上「00 : 25」的右下角
        Vector2 referencePos = timeTextReferencePoint.anchoredPosition; // 取得 00:25 的位置
        // 在右下角加一點點偏移量 (例如往右 100, 往下 50，你可以手動調這裡的數字直到順眼為止)
        Vector2 spawnPosition = referencePos + new Vector2(100f, -50f); 

        // 2. 「生出」飄字實體 (要生在 timeTextReferencePoint 的爸爸底下，通常是 Canvas，確保圖層順序正确)
        GameObject popup = Instantiate(timePopupPrefab, timeTextReferencePoint.parent);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        
        // 3. 設定飄字初始位置
        if (popupRect != null)
        {
            popupRect.anchoredPosition = spawnPosition;
        }

        // 4. 【完美設定文字內容】將 "+10"、"+30" 寫入文字中
        Text textComponent = popup.GetComponent<Text>();
        if (textComponent != null)
        {
            // 使用 Mathf.Ceil 無條件進位顯示整數 (例如 30f -> "30")
            textComponent.text = $"+{Mathf.Ceil(amount)}";
        }
        
        Debug.Log($"<b>🖼️【加時飄字】顯示效果: {textComponent.text}</b>");
    }

    private void ClearSlot(int index)
    {
        if (index < 0 || index >= currentItems.Length) return;
        currentItems[index] = null;
        itemIcons[index].sprite = null;
        itemIcons[index].color = new Color(1, 1, 1, 0); 
    }
}
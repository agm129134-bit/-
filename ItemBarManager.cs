using System.Collections; // 為了使用協程一定要加這行！
using UnityEngine;
using UnityEngine.UI;

// ==========================================
// 🎒 腳本 3：升級版道具欄管理員 (融合鍵盤、加速、沙漏、美人魚)
// ==========================================
public class ItemBarManager : MonoBehaviour
{
    [Header("UI 設定")]
    [Tooltip("把 Slot_1 到 Slot_3 裡面的 ItemIcon 拖曳進來")]
    public Image[] itemIcons; 

    // ----------------------------------------------------
    [Header("⏳ 加時道具設定 (沙漏/瓶子)")]
    [Tooltip("請把沙漏(增加時間)的圖片拖進來")]
    public Sprite hourglassSprite; 
    [Tooltip("使用沙漏可以加多少秒遊戲時間？")]
    public float addTimeSeconds = 30f; 

    // ----------------------------------------------------
    [Header("👟 加速道具設定 (紅鞋子)")]
    [Tooltip("請把紅鞋子的圖片拖進來")]
    public Sprite shoeSprite; 
    [Tooltip("加速效果持續幾秒？")]
    public float speedBoostDuration = 5f;
    [Tooltip("加速的倍率")]
    public float speedMultiplier = 1.5f;

    [Tooltip("請把地圖上的玩家物件拖曳進來")]
    public GameObject playerObject; 

    // ----------------------------------------------------
    // 🌟 【新增區塊】美人魚照片道具設定 ( image_21.png )
    // ----------------------------------------------------
    [Header("🧜‍♀️ 美人魚照片道具設定")]
    [Tooltip("請把美人魚照片圖片 (image_21.png) 拖進來，讓系統知道誰是照片")]
    public Sprite mermaidPhotoSprite;

    [Tooltip("美人魚照片持續幾秒？ (大魚停止移動的秒數)")]
    public float photoFreezeDuration = 5f;

    [Tooltip("請把場景中的 PhotoOverlayManager 物件拖進來 (為了顯示全螢幕照片)")]
    public PhotoOverlayManager photoOverlayManager;

    [Tooltip("請把地圖上的紅魚物件 ( image_13.png 中的魚) 拖曳進來 (假設魚有 FishMovement 腳本)")]
    public GameObject fishObject; 
    // ----------------------------------------------------

    // 內部記錄目前每個格子裝什麼道具
    private Sprite[] currentItems;
    private int currentSelectedIndex = 0; 
    private Coroutine photoCoroutine; // 用來記錄目前的照片協程

    void Start()
    {
        currentItems = new Sprite[itemIcons.Length];
        
        // 遊戲一開始，清空
        for (int i = 0; i < itemIcons.Length; i++)
        {
            ClearSlot(i);
        }
        SelectSlot(0);
    }

    // 鍵盤操作偵測
    void Update()
    {
        // 1, 2, 3 切換
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SelectSlot(2);

        // Enter 使用
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            UseItem(currentSelectedIndex);
        }
    }

    // 格子選取特效
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
                    if (i == currentSelectedIndex)
                    {
                        slotParent.localScale = new Vector3(1.15f, 1.15f, 1f);
                    }
                    else
                    {
                        slotParent.localScale = new Vector3(1f, 1f, 1f);
                    }
                }
            }
        }
    }

    // 獲得道具
    public bool AddItem(Sprite newItemSprite)
    {
        for (int i = 0; i < currentItems.Length; i++)
        {
            if (currentItems[i] == null)
            {
                currentItems[i] = newItemSprite;      
                itemIcons[i].sprite = newItemSprite;  
                itemIcons[i].color = new Color(1, 1, 1, 1); 
                
                Debug.Log($"成功獲得道具，放在第 {i + 1} 格！");
                return true; 
            }
        }
        
        Debug.Log("道具欄滿了！放不下啦！");
        return false; 
    }

    // 使用道具 Core Logic
    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentItems.Length) return;

        Sprite itemToUse = currentItems[slotIndex];

        if (itemToUse != null)
        {
            Debug.Log($"準備使用第 {slotIndex + 1} 格的道具...");
            
            // 判定 1：加時道具 (沙漏)
            if (hourglassSprite != null && itemToUse == hourglassSprite)
            {
                if (GameTimer.Instance != null)
                {
                    GameTimer.Instance.AddGameTime(addTimeSeconds);
                    Debug.Log($"⏳ 使用加時道具！遊戲時間增加了 {addTimeSeconds} 秒！");
                }
                else { Debug.LogError("🚨 找不到 GameTimer，無法加時間！"); }
            }
            // 判定 2：加速道具 (鞋子)
            else if (shoeSprite != null && itemToUse == shoeSprite)
            {
                if (playerObject != null)
                {
                    PlayerMovement pm = playerObject.GetComponent<PlayerMovement>();
                    if (pm != null) { pm.ActivateSpeedBoost(speedMultiplier, speedBoostDuration); Debug.Log($"👟 使用加速道具！玩家加速 {speedMultiplier} 倍！"); }
                    else { Debug.LogError("🚨 Player 物件上找不到 'PlayerMovement' 腳本！無法加速！"); }
                }
                else { Debug.LogWarning("🚨 找不到 Player 物件，無法執行加速！"); }
            }
            // 🌟 ==========================================
            // 🌟 【新增魔法】判定 3：使用的是【美人魚照片道具】嗎？
            // 🌟 ==========================================
            else if (mermaidPhotoSprite != null && itemToUse == mermaidPhotoSprite)
            {
                Debug.Log("🧜‍♀️ 使用美人魚照片！魚將停在原地 5 秒，畫面被照片佔據！");

                // 防呆：如果照片協程已經在跑，就不要重複啟動
                if (photoCoroutine != null) StopCoroutine(photoCoroutine);
                
                // 啟動新的協程：處理魚凍結和照片覆蓋
                photoCoroutine = StartCoroutine(MermaidPhotoRoutine());
                
                // 如果照片協程結束，將參考歸零
                // photoCoroutine = null; // 錯誤的寫法，不能直接在協程啟動後歸零。必須在協程內部的最後歸零。
            }
            // ==========================================
            // 判定 4：其他道具
            else
            {
                Debug.Log("使用了別的道具（非沙漏、非鞋子、非照片）");
            }

            // 使用完後清空該格子
            ClearSlot(slotIndex);
        }
        else { Debug.Log($"第 {slotIndex + 1} 格是空的！不能使用！"); }
    }

    // ==========================================
    // 🌟 【新增魔法：美人魚照片協程 (碼表邏輯)】
    // 負責處理魚凍結 5 秒和照片佔據畫面 5 秒。
    // ==========================================
    IEnumerator MermaidPhotoRoutine()
    {
        // ------------------------------------------
        // **步驟 1：凍結魚**
        // 抓出地圖上的紅魚身上的 FishMovement 移動腳本
        // ------------------------------------------
        FishMovement fm = null;
        if (fishObject != null)
        {
            fm = fishObject.GetComponent<FishMovement>();
            if (fm != null)
            {
                fm.canMove = false; // 按下凍結按鈕！讓魚停在原地。
                Debug.Log("<color=red><b>🥶【魚凍結】魚停在原地 5 秒！</b></color>");
            }
            else
            {
                Debug.LogError("🚨 雖然找到了魚物件，但上面找不到名叫 'FishMovement' 的腳本！無法凍結魚！");
            }
        }
        else { Debug.LogWarning("🚨 找不到地圖上的魚物件，無法凍結！請在 Inspector 綁定 Fish Object。"); }

        // ------------------------------------------
        // **步驟 2：顯示全螢幕照片**
        // 呼叫照片管理員顯示照片
        // ------------------------------------------
        if (photoOverlayManager != null)
        {
            photoOverlayManager.ShowPhoto();
            Debug.Log("<b>🖼️【照片覆蓋】美人魚照片佔據整個螢幕，玩家無法看到魚的狀況！</b>");
        }
        else { Debug.LogError("🚨 找不到 PhotoOverlayManager，無法顯示全螢幕照片！"); }

        // ------------------------------------------
        // **步驟 3：等待 5 秒 (photoFreezeDuration)**
        // 協程在此停頓 5 秒，讓魚保持凍結，畫面保持覆蓋
        // ------------------------------------------
        yield return new WaitForSeconds(photoFreezeDuration);

        // ------------------------------------------
        // **步驟 4：解凍魚**
        // 碼表時間到！把魚的 canMove 設回原本的樣子
        // ------------------------------------------
        if (fm != null)
        {
            fm.canMove = true; // 解凍！恢復魚移動。
            Debug.Log("<color=white><b>🔙【魚解凍】魚恢復移動！</b></color>");
        }

        // ------------------------------------------
        // **步驟 5：隱藏全螢幕照片**
        // 移除照片覆蓋，恢復遊戲畫面
        // ------------------------------------------
        if (photoOverlayManager != null)
        {
            photoOverlayManager.HidePhoto();
            Debug.Log("<b>🔙【照片移除】美人魚照片移除，遊戲恢復正常畫面！</b>");
        }

        // 將照片協程參考歸零，代表可以下一次使用
        photoCoroutine = null;
    }

    // 清空格子
    private void ClearSlot(int index)
    {
        if (index < 0 || index >= currentItems.Length) return;
        currentItems[index] = null;
        itemIcons[index].sprite = null;
        itemIcons[index].color = new Color(1, 1, 1, 0); 
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemBarManager : MonoBehaviour
{
    [Header("UI 設定")]
    [Tooltip("把 Slot_1 到 Slot_3 裡面的 ItemIcon 拖曳進來")]
    public Image[] itemIcons; 

    // ----------------------------------------------------
    [Header("⏳ 加時道具設定 (沙漏/瓶子)")]
    public Sprite hourglassSprite; 
    public float addTimeSeconds = 30f; 

    // ----------------------------------------------------
    [Header("👟 加速道具設定 (紅鞋子)")]
    public Sprite shoeSprite; 
    public float speedBoostDuration = 5f;
    public float speedMultiplier = 1.5f;
    public GameObject playerObject; 

    // ----------------------------------------------------
    // 🌟 美人魚照片道具設定 (螢幕遮擋 + 凍結)
    // ----------------------------------------------------
    [Header("🧜‍♀️ 美人魚照片道具設定")]
    [Tooltip("請把美人魚照片圖片拖進來，讓系統認識這個道具")]
    public Sprite mermaidPhotoSprite;

    [Tooltip("照片佔據畫面持續幾秒？ (定身秒數)")]
    public float photoFreezeDuration = 5f;

    [Tooltip("請把左側 Hierarchy 的 PhotoOverlayManager 拖進來 (負責顯示 UI 照片)")]
    public PhotoOverlayManager photoOverlayManager;

    [Tooltip("請把地圖上要被凍結的「紅魚物件」拖進來")]
    public GameObject targetToFreeze; 
    // ----------------------------------------------------

    private Sprite[] currentItems;
    private int currentSelectedIndex = 0; 
    private Coroutine photoCoroutine; 

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
                Debug.Log($"成功獲得道具，放在第 {i + 1} 格！");
                return true; 
            }
        }
        Debug.Log("道具欄滿了！放不下啦！");
        return false; 
    }

    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentItems.Length) return;

        Sprite itemToUse = currentItems[slotIndex];

        if (itemToUse != null)
        {
            if (hourglassSprite != null && itemToUse == hourglassSprite)
            {
                if (GameTimer.Instance != null) GameTimer.Instance.AddGameTime(addTimeSeconds);
            }
            else if (shoeSprite != null && itemToUse == shoeSprite)
            {
                if (playerObject != null)
                {
                    PlayerMovement pm = playerObject.GetComponent<PlayerMovement>();
                    if (pm != null) pm.ActivateSpeedBoost(speedMultiplier, speedBoostDuration);
                }
            }
            // 🌟 ==========================================
            // 判定 3：【美人魚照片】
            // 移除了限制，現在每一張照片都能觸發效果！
            // 🌟 ==========================================
            else if (mermaidPhotoSprite != null && itemToUse == mermaidPhotoSprite)
            {
                // 如果前一張照片的效果還在跑，就先停掉舊的碼表
                if (photoCoroutine != null) StopCoroutine(photoCoroutine);
                
                // 重新啟動一次 5 秒的定身與遮蔽！
                photoCoroutine = StartCoroutine(MermaidPhotoRoutine());
            }

            // 無論有沒有效果，用完道具都會清空該格子
            ClearSlot(slotIndex);
        }
    }

    // ==========================================
    // 🌟 處理畫面遮擋與目標凍結的協程
    // ==========================================
    IEnumerator MermaidPhotoRoutine()
    {
        FishMovement fm = null;

        // 1. 凍結目標
        if (targetToFreeze != null)
        {
            fm = targetToFreeze.GetComponent<FishMovement>();
            if (fm != null)
            {
                fm.canMove = false;
                Debug.Log("<color=red><b>🥶【目標凍結】魚停止移動！</b></color>");
            }
        }

        // 2. 顯示 Canvas UI 照片
        if (photoOverlayManager != null)
        {
            photoOverlayManager.ShowPhoto();
            Debug.Log("<b>🖼️【照片遮擋】美人魚照片彈出，佔據畫面！</b>");
        }

        // 3. 等待 5 秒
        yield return new WaitForSeconds(photoFreezeDuration);

        // 4. 解凍目標
        if (fm != null)
        {
            fm.canMove = true;
            Debug.Log("<color=white><b>🔙【目標解凍】魚恢復自由！</b></color>");
        }

        // 5. 隱藏 Canvas UI 照片
        if (photoOverlayManager != null)
        {
            photoOverlayManager.HidePhoto();
            Debug.Log("<b>🔙【畫面恢復】照片移除！</b>");
        }

        photoCoroutine = null;
    }

    private void ClearSlot(int index)
    {
        if (index < 0 || index >= currentItems.Length) return;
        currentItems[index] = null;
        itemIcons[index].sprite = null;
        itemIcons[index].color = new Color(1, 1, 1, 0); 
    }
}
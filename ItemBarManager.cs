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

    [Header("⏳ 加時飄字特效設定")]
    public GameObject timePopupPrefab; 
    public RectTransform timeTextReferencePoint; 

    // ----------------------------------------------------
    // 👟 加速道具設定 (紅鞋子)
    // ----------------------------------------------------
    [Header("👟 加速道具設定 (紅鞋子)")]
    public Sprite shoeSprite; 
    public float speedBoostDuration = 5f;
    public float speedMultiplier = 1.5f;
    public GameObject playerObject; 

    // ----------------------------------------------------
    // 📸 大魚照片道具設定 (視覺干擾 + 凍結)
    // ----------------------------------------------------
    public enum PhotoTarget { FishScreen, HumanScreen, BothScreens }

    [Header("📸 照片道具設定 (大魚照片)")]
    public Sprite fishPhotoSprite; 
    public PhotoTarget targetScreen = PhotoTarget.FishScreen;
    public float photoDuration = 3f;
    public GameObject bigPhotoOnFishScreen;
    public GameObject bigPhotoOnHumanScreen;

    // 🌟 【超級新增】凍結目標設定
    [Header("❄️ 附加效果：凍結不能動")]
    [Tooltip("請把你想凍結的對象(大魚或人類)的『移動腳本』拖進來")]
    public MonoBehaviour movementScriptToFreeze;

    private Coroutine photoCoroutine; 
    private Sprite[] currentItems;
    private int currentSelectedIndex = 0; 

    void Start()
    {
        currentItems = new Sprite[itemIcons.Length];
        for (int i = 0; i < itemIcons.Length; i++) ClearSlot(i);
        SelectSlot(0);

        if (bigPhotoOnFishScreen != null) bigPhotoOnFishScreen.SetActive(false);
        if (bigPhotoOnHumanScreen != null) bigPhotoOnHumanScreen.SetActive(false);
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
            if (hourglassSprite != null && itemToUse == hourglassSprite)
            {
                if (GameTimer.Instance != null)
                {
                    GameTimer.Instance.AddGameTime(addTimeSeconds);
                    TriggerTimePopupEffect(addTimeSeconds);
                }
            }
            else if (shoeSprite != null && itemToUse == shoeSprite)
            {
                if (playerObject != null)
                {
                    PlayerMovement pm = playerObject.GetComponent<PlayerMovement>();
                    if (pm != null) pm.ActivateSpeedBoost(speedMultiplier, speedBoostDuration);
                }
            }
            else if (fishPhotoSprite != null && itemToUse == fishPhotoSprite)
            {
                Debug.Log("📸 玩家使用了大魚照片！視覺干擾 + 凍結 發動！");
                
                if (photoCoroutine != null) StopCoroutine(photoCoroutine);
                photoCoroutine = StartCoroutine(ShowBigPhotoRoutine());
            }

            ClearSlot(slotIndex);
        }
    }

    // ==========================================
    // 🌟 控制大照片與凍結效果的協程
    // ==========================================
    private IEnumerator ShowBigPhotoRoutine()
    {
        // 1. 顯示照片
        if (targetScreen == PhotoTarget.FishScreen || targetScreen == PhotoTarget.BothScreens)
            if (bigPhotoOnFishScreen != null) bigPhotoOnFishScreen.SetActive(true);

        if (targetScreen == PhotoTarget.HumanScreen || targetScreen == PhotoTarget.BothScreens)
            if (bigPhotoOnHumanScreen != null) bigPhotoOnHumanScreen.SetActive(true);

        // ❄️ 2. 【核心魔法】凍結角色！直接關閉它的移動腳本
        if (movementScriptToFreeze != null)
        {
            movementScriptToFreeze.enabled = false;
        }

        // 3. 乖乖等照片停留的時間
        yield return new WaitForSeconds(photoDuration);

        // 4. 時間到，把照片藏起來
        if (bigPhotoOnFishScreen != null) bigPhotoOnFishScreen.SetActive(false);
        if (bigPhotoOnHumanScreen != null) bigPhotoOnHumanScreen.SetActive(false);
        
        // 🏃‍♂️ 5. 【核心魔法】解除凍結！重新打開它的移動腳本
        if (movementScriptToFreeze != null)
        {
            movementScriptToFreeze.enabled = true;
        }

        photoCoroutine = null;
    }

    private void TriggerTimePopupEffect(float amount)
    {
        if (timePopupPrefab == null || timeTextReferencePoint == null) return;

        Vector2 referencePos = timeTextReferencePoint.anchoredPosition; 
        Vector2 spawnPosition = referencePos + new Vector2(100f, -50f); 

        GameObject popup = Instantiate(timePopupPrefab, timeTextReferencePoint.parent);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        
        if (popupRect != null) popupRect.anchoredPosition = spawnPosition;

        Text textComponent = popup.GetComponent<Text>();
        if (textComponent != null) textComponent.text = $"+{Mathf.Ceil(amount)}";
    }

    private void ClearSlot(int index)
    {
        if (index < 0 || index >= currentItems.Length) return;
        currentItems[index] = null;
        itemIcons[index].sprite = null;
        itemIcons[index].color = new Color(1, 1, 1, 0); 
    }
}
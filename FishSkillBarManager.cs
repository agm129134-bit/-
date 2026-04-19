using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 🌟 要求這個腳本所在的物件一定要有 AudioSource 元件，用來發出聲音
[RequireComponent(typeof(AudioSource))]
public class FishSkillBarManager : MonoBehaviour
{
    // ==========================================
    // 🌟 左下角 UI 狀態列設定
    // ==========================================
    [Header("左下角技能欄 UI 設定")]
    public Image[] skillIcons;
    public Image[] cooldownMasks;
    public Text[] cooldownTexts; 

    // ==========================================
    // 🌟 右下角「罰！」大按鈕 UI 設定
    // ==========================================
    [Header("右下角「罰！」攻擊按鈕 UI 設定")]
    public Button attackButton;           
    public Image buttonBaseImage;         
    public Image buttonCooldownOverlay;   
    public Sprite readySprite;            
    public Sprite cooldownSprite;         

    // ==========================================
    // 🐟 技能 1 設定 (雷達追蹤)
    // ==========================================
    [Header("🐟 技能 1 設定 (雷達追蹤)")]
    public float skill1Cooldown = 60f; 
    public float skill1Duration = 8f;  
    
    public GameObject trackingArrow;   
    public Transform humanTransform;   
    public Transform fishTransform;    
    public float orbitDistance = 2.0f; 

    [Header("✨ 技能 1 附加效果：人類高亮")]
    public bool enableHumanHighlight = true; 
    public SpriteRenderer humanSpriteRenderer; 
    public Color highlightColor = Color.red;
    private Color originalHumanColor; 

    private float skill1CurrentCD = 0f; 
    private bool isTracking = false;    

    // ==========================================
    // 💦 技能 2 設定 (原汁原味的口水彈)
    // ==========================================
    [Header("💦 技能 2 設定 (口水彈)")]
    public GameObject spitPrefab; 
    public Transform firePoint;   
    public bool reverseShootDirection = false; 
    public float skill2Cooldown = 20f; // 恢復你原本設定的 20 秒冷卻
    public AudioClip spitSound; // 吐口水音效

    private float skill2CurrentCD = 0f; 
    private int currentSelectedIndex = 0; 

    // ==========================================
    // ⚡ 額外攻擊設定 (獨立的「罰！」按鈕)
    // ==========================================
    [Header("⚡ 額外攻擊設定 (右下角『罰』)")]
    [Tooltip("🌟 罰的冷卻時間 (10 秒)")]
    public float punishCooldown = 10f;
    [Tooltip("請把大魚『罰』的攻擊音效拖進來")]
    public AudioClip punishSound;

    private float punishCurrentCD = 0f; // 獨立的「罰」冷卻計時器
    private AudioSource audioSource; 

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        SelectSlot(0);
        
        if (cooldownMasks.Length > 0 && cooldownMasks[0] != null) cooldownMasks[0].fillAmount = 0;
        if (cooldownMasks.Length > 1 && cooldownMasks[1] != null) cooldownMasks[1].fillAmount = 0;
        if (cooldownTexts.Length > 0 && cooldownTexts[0] != null) cooldownTexts[0].text = "";
        if (cooldownTexts.Length > 1 && cooldownTexts[1] != null) cooldownTexts[1].text = "";

        if (buttonCooldownOverlay != null) buttonCooldownOverlay.fillAmount = 0;
        if (buttonBaseImage != null && readySprite != null) buttonBaseImage.sprite = readySprite;

        if (trackingArrow != null) trackingArrow.SetActive(false);
    }

    void Update()
    {
        // ----------------------------------------------------
        // 🌟 衛星環繞魔法 (技能1)
        // ----------------------------------------------------
        if (isTracking && trackingArrow != null && humanTransform != null && fishTransform != null)
        {
            Vector2 direction = humanTransform.position - fishTransform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            trackingArrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            trackingArrow.transform.position = fishTransform.position + (Vector3)direction.normalized * orbitDistance;
        }

        // ----------------------------------------------------
        // ⏳ 處理冷卻時間倒數與所有 UI 更新
        // ----------------------------------------------------
        
        // 【左下角技能 1 (雷達)】
        if (skill1CurrentCD > 0)
        {
            skill1CurrentCD -= Time.deltaTime; 
            if (cooldownMasks.Length > 0 && cooldownMasks[0] != null) 
                cooldownMasks[0].fillAmount = skill1CurrentCD / skill1Cooldown; 
            if (cooldownTexts.Length > 0 && cooldownTexts[0] != null)
                cooldownTexts[0].text = Mathf.Ceil(skill1CurrentCD).ToString(); 
        }
        else if (cooldownTexts.Length > 0 && cooldownTexts[0] != null) cooldownTexts[0].text = ""; 

        // 【左下角技能 2 (口水彈)】
        if (skill2CurrentCD > 0)
        {
            skill2CurrentCD -= Time.deltaTime;
            if (cooldownMasks.Length > 1 && cooldownMasks[1] != null) 
                cooldownMasks[1].fillAmount = skill2CurrentCD / skill2Cooldown;
            if (cooldownTexts.Length > 1 && cooldownTexts[1] != null)
                cooldownTexts[1].text = Mathf.Ceil(skill2CurrentCD).ToString();
        }
        else if (cooldownTexts.Length > 1 && cooldownTexts[1] != null) cooldownTexts[1].text = "";

        // 【右下角「罰」大按鈕的獨立冷卻】
        if (punishCurrentCD > 0)
        {
            punishCurrentCD -= Time.deltaTime;
            
            if (buttonCooldownOverlay != null) buttonCooldownOverlay.fillAmount = punishCurrentCD / punishCooldown;
            if (attackButton != null) attackButton.interactable = false; 
        }
        else 
        {
            if (buttonCooldownOverlay != null) buttonCooldownOverlay.fillAmount = 0;
            if (attackButton != null) attackButton.interactable = true;
            if (buttonBaseImage != null && readySprite != null) buttonBaseImage.sprite = readySprite; 
        }

        // ----------------------------------------------------
        // ⌨️ 鍵盤操作 (控制左下角的技能)
        // ----------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SelectSlot(0); 
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SelectSlot(1); 
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) UseSkill(currentSelectedIndex); 
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= skillIcons.Length) return;
        currentSelectedIndex = index;
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i] != null)
            {
                skillIcons[i].transform.localScale = i == currentSelectedIndex ? new Vector3(1.15f, 1.15f, 1f) : new Vector3(1f, 1f, 1f);
            }
        }
    }

    public void UseSkill(int slotIndex)
    {
        if (slotIndex == 0 && skill1CurrentCD <= 0)
        {
            skill1CurrentCD = skill1Cooldown; 
            StartCoroutine(TrackHumanRoutine()); 
        }
        else if (slotIndex == 1 && skill2CurrentCD <= 0) 
        {
            CastSpitSkill(); // 原汁原味的吐口水！
        }
    }

    private IEnumerator TrackHumanRoutine()
    {
        if (trackingArrow != null)
        {
            isTracking = true;
            trackingArrow.SetActive(true); 
        }

        if (enableHumanHighlight && humanSpriteRenderer != null)
        {
            originalHumanColor = humanSpriteRenderer.color; 
            humanSpriteRenderer.color = highlightColor;     
        }

        yield return new WaitForSeconds(skill1Duration); 

        if (trackingArrow != null)
        {
            trackingArrow.SetActive(false); 
            isTracking = false;
        }

        if (enableHumanHighlight && humanSpriteRenderer != null)
        {
            humanSpriteRenderer.color = originalHumanColor; 
        }
    }

    // ==========================================
    // 💦 實際發射口水彈的函數 (技能 2)
    // ==========================================
    private void CastSpitSkill()
    {
        if (spitPrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(spitPrefab, firePoint.position, Quaternion.identity);
            
            SpriteRenderer fishSprite = firePoint.GetComponentInParent<SpriteRenderer>();
            Vector2 shootDirection = Vector2.right; 
            
            if (fishSprite != null && fishSprite.flipX) shootDirection = Vector2.left; 
            if (reverseShootDirection) shootDirection = -shootDirection; 

            SpitProjectile sp = projectile.GetComponent<SpitProjectile>();
            if (sp != null) sp.SetDirection(shootDirection);
            
            skill2CurrentCD = skill2Cooldown;

            if (spitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(spitSound);
            }
            
            Debug.Log("💦 大魚發射了一發口水彈！");
        }
    }

    // ==========================================
    // ⚡ 直接「罰！」扣血的專用接口 (右下角大按鈕專用)
    // ==========================================
    public void ClickToPunish()
    {
        // 檢查罰的冷卻時間到了沒
        if (punishCurrentCD <= 0)
        {
            if (GameManager.Instance != null)
            {
                int targetPlayerId = 0; // 預設攻擊 1P
                GameManager.Instance.TakeDamage(targetPlayerId);

                // 🌟 進入獨立的 10 秒冷卻時間
                punishCurrentCD = punishCooldown;

                // 把大按鈕的圖片換成灰色冷卻版
                if (buttonBaseImage != null && cooldownSprite != null)
                {
                    buttonBaseImage.sprite = cooldownSprite;
                }

                if (punishSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(punishSound);
                }
                
                Debug.Log("⚡ 大魚發動了『罰』！人類玩家直接扣除一滴血！");
            }
        }
        else
        {
             Debug.Log("『罰』還在冷卻中！");
        }
    }
}
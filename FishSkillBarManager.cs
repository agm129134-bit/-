using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 🌟 【新增】要求這個腳本所在的物件一定要有 AudioSource 元件，用來發出聲音
[RequireComponent(typeof(AudioSource))]
public class FishSkillBarManager : MonoBehaviour
{
    [Header("UI 設定")]
    public Image[] skillIcons;
    public Image[] cooldownMasks;
    public Text[] cooldownTexts; 

    // ==========================================
    // 🌟 技能 1 設定 (雷達追蹤箭頭)
    // ==========================================
    [Header("🐟 技能 1 設定 (雷達追蹤)")]
    public float skill1Cooldown = 60f; 
    public float skill1Duration = 8f;  
    
    [Tooltip("請把『追蹤箭頭』物件拖進來")]
    public GameObject trackingArrow;   
    [Tooltip("請把『人類玩家』拖進來")]
    public Transform humanTransform;   
    [Tooltip("請把『大魚自己』拖進來")]
    public Transform fishTransform;    
    
    [Tooltip("箭頭距離大魚中心點有多遠？")]
    public float orbitDistance = 2.0f; 

    // ==========================================
    // ✨ 新增：高亮設定 (人類玩家)
    // ==========================================
    [Header("✨ 技能 1 附加效果：人類高亮")]
    [Tooltip("打勾：開啟高亮 / 取消打勾：關閉高亮")]
    public bool enableHumanHighlight = true; 
    
    [Tooltip("請把『人類玩家』物件拖進來 (需要抓取他身上的圖片來變色)")]
    public SpriteRenderer humanSpriteRenderer; 
    
    [Tooltip("你要讓人類變成什麼顏色？(預設紅色)")]
    public Color highlightColor = Color.red;
    
    private Color originalHumanColor; // 用來記住人類原本的顏色

    private float skill1CurrentCD = 0f;
    private bool isTracking = false; 

    // ==========================================
    // 💦 技能 2 設定 (口水彈)
    // ==========================================
    [Header("💦 技能 2 設定 (口水彈)")]
    public GameObject spitPrefab;
    public Transform firePoint;
    public bool reverseShootDirection = false;
    public float skill2Cooldown = 20f;

    // ==========================================
    // 🌟 【新增】技能 2 音效設定
    // ==========================================
    [Tooltip("請把大魚吐口水的音效拖進來")]
    public AudioClip spitSound;
    private AudioSource audioSource; // 這是用來播放聲音的喇叭

    private float skill2CurrentCD = 0f;

    private int currentSelectedIndex = 0;

    // 🌟 【新增】在程式一開始先抓取身上的喇叭
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

        if (trackingArrow != null) trackingArrow.SetActive(false);
    }

    void Update()
    {
        // 🌟 衛星環繞魔法
        if (isTracking && trackingArrow != null && humanTransform != null && fishTransform != null)
        {
            Vector2 direction = humanTransform.position - fishTransform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            trackingArrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            trackingArrow.transform.position = fishTransform.position + (Vector3)direction.normalized * orbitDistance;
        }

        // 處理冷卻與文字
        if (skill1CurrentCD > 0)
        {
            skill1CurrentCD -= Time.deltaTime;
            if (cooldownMasks.Length > 0 && cooldownMasks[0] != null) 
                cooldownMasks[0].fillAmount = skill1CurrentCD / skill1Cooldown;
            if (cooldownTexts.Length > 0 && cooldownTexts[0] != null)
                cooldownTexts[0].text = Mathf.Ceil(skill1CurrentCD).ToString();
        }
        else if (cooldownTexts.Length > 0 && cooldownTexts[0] != null) cooldownTexts[0].text = "";

        if (skill2CurrentCD > 0)
        {
            skill2CurrentCD -= Time.deltaTime;
            if (cooldownMasks.Length > 1 && cooldownMasks[1] != null) 
                cooldownMasks[1].fillAmount = skill2CurrentCD / skill2Cooldown;
            if (cooldownTexts.Length > 1 && cooldownTexts[1] != null)
                cooldownTexts[1].text = Mathf.Ceil(skill2CurrentCD).ToString();
        }
        else if (cooldownTexts.Length > 1 && cooldownTexts[1] != null) cooldownTexts[1].text = "";

        // 鍵盤切換與發射
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
                skillIcons[i].transform.localScale = i == currentSelectedIndex ? new Vector3(1.15f, 1.15f, 1f) : new Vector3(1f, 1f, 1f);
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
            CastSpitSkill();
        }
    }

    // ==========================================
    // 🌟 控制追蹤與高亮時間的協程
    // ==========================================
    private IEnumerator TrackHumanRoutine()
    {
        // 1. 啟動追蹤箭頭
        if (trackingArrow != null)
        {
            isTracking = true;
            trackingArrow.SetActive(true); 
        }

        // 2. 啟動人類高亮 (如果開關有打勾的話)
        if (enableHumanHighlight && humanSpriteRenderer != null)
        {
            originalHumanColor = humanSpriteRenderer.color; // 先偷偷記住人類原本的顏色 (通常是白色)
            humanSpriteRenderer.color = highlightColor;     // 幫他換上警戒色！
        }

        // 3. 乖乖等 8 秒
        yield return new WaitForSeconds(skill1Duration); 

        // 4. 時間到，關閉追蹤箭頭
        if (trackingArrow != null)
        {
            trackingArrow.SetActive(false); 
            isTracking = false;
        }

        // 5. 時間到，把人類的顏色變回來
        if (enableHumanHighlight && humanSpriteRenderer != null)
        {
            humanSpriteRenderer.color = originalHumanColor; 
        }
    }

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

            // ==========================================
            // 🌟 【新增】發射時播放吐口水音效！
            // ==========================================
            if (spitSound != null && audioSource != null)
            {
                // PlayOneShot 適合這種短暫的音效
                audioSource.PlayOneShot(spitSound);
            }
        }
    }
}
using UnityEngine;
using System.Collections; 

public class FishMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float fishSpeed = 3f;
    
    [Tooltip("魚會在這個半徑範圍內隨機亂游 (避免游出地圖)")]
    public float wanderRadius = 10f; 

    [Header("休息設定")]
    [Tooltip("游到目的地後，最少停留在原地休息幾秒？")]
    public float minWaitTime = 1f;
    [Tooltip("游到目的地後，最多休息幾秒？")]
    public float maxWaitTime = 3f;

    [Header("圖片設定 (選填)")]
    [Tooltip("如果你的魚原圖是『頭朝右邊』，請打勾；如果是『頭朝左邊』，請取消打勾")]
    public bool isFacingRightOriginally = false; 
    private SpriteRenderer spriteRenderer;

    // ==========================================
    // 🌟 攻擊設定 (已加入自訂開關)
    // ==========================================
    [Header("攻擊設定")]
    [Tooltip("🌟 打勾：碰到玩家會自動咬人扣血 / 取消打勾：變成和平模式，撞到也不會扣血")]
    public bool enableAutoAttack = true; 

    [Tooltip("咬人一次之後，要間隔幾秒才能再咬下一次？(防秒殺保護)")]
    public float damageCooldown = 2f; 
    private float lastDamageTime = -999f; // 記錄上次咬人的時間

    // 🌟 【核心魔法：凍結變數】
    [HideInInspector] 
    public bool canMove = true;

    // 內部運算變數
    private Vector2 startPosition;  
    private Vector2 targetPosition; 
    private bool isWaiting = false; 

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        PickNewTargetPosition();
    }

    void Update()
    {
        if (!canMove || isWaiting) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fishSpeed * Time.deltaTime);
        UpdateFacingDirection();

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            StartCoroutine(WaitAndPickNewPosition());
        }
    }

    // 隨機在地圖上找一個新座標
    private void PickNewTargetPosition()
    {
        float randomX = Random.Range(-wanderRadius, wanderRadius);
        float randomY = Random.Range(-wanderRadius, wanderRadius);
        targetPosition = startPosition + new Vector2(randomX, randomY);
    }

    // 休息倒數的碼表
    private IEnumerator WaitAndPickNewPosition()
    {
        isWaiting = true; 
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime); 
        
        PickNewTargetPosition();
        isWaiting = false; 
    }

    // 處理圖片左右翻轉的邏輯
    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;

        if (targetPosition.x > transform.position.x)
        {
            spriteRenderer.flipX = !isFacingRightOriginally;
        }
        else if (targetPosition.x < transform.position.x)
        {
            spriteRenderer.flipX = isFacingRightOriginally;
        }
    }

    // ==========================================
    // 🌟 碰到玩家扣血邏輯
    // ==========================================
    
    // 如果你的大魚碰撞體勾選了 "Is Trigger"，會觸發這個
    private void OnTriggerEnter2D(Collider2D other) { TryDealDamage(other.gameObject); }
    private void OnTriggerStay2D(Collider2D other) { TryDealDamage(other.gameObject); }

    // 如果你的大魚碰撞體是實體的 (沒有勾選 Is Trigger)，會觸發這個
    private void OnCollisionEnter2D(Collision2D collision) { TryDealDamage(collision.gameObject); }
    private void OnCollisionStay2D(Collision2D collision) { TryDealDamage(collision.gameObject); }

    // 統一處理扣血的函數
    private void TryDealDamage(GameObject hitObject)
    {
        // 🌟 【新增】如果沒有開啟自動攻擊，就直接退出，當作沒碰到！
        if (!enableAutoAttack) return;

        // 檢查碰到的東西是不是玩家
        if (hitObject.CompareTag("Player"))
        {
            // 檢查冷卻時間到了沒
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                if (GameManager.Instance != null)
                {
                    // ⚠️ 注意：這裡預設扣 1P (ID:0) 的血。
                    // 如果你們有雙人系統，可以從 hitObject 身上讀取玩家 ID 傳進去
                    int playerId = 0; 
                    GameManager.Instance.TakeDamage(playerId);
                    
                    // 更新最後一次咬人的時間，重新開始冷卻
                    lastDamageTime = Time.time; 
                    
                    Debug.Log("🐟 大魚咬了玩家！扣一滴血！");
                }
            }
        }
    }
}
using UnityEngine;
using System.Collections; // 為了使用協程等待，一定要加這行！

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
    public bool isFacingRightOriginally = false; // 預設多數 2D 素材是朝左
    private SpriteRenderer spriteRenderer;

    // 🌟 【核心魔法：凍結變數】
    [HideInInspector] 
    public bool canMove = true;

    // 內部運算變數
    private Vector2 startPosition;  // 記錄魚一開始出生的位置 (作為活動中心點)
    private Vector2 targetPosition; // 魚目前想去的地方
    private bool isWaiting = false; // 記錄魚是不是正在休息

    void Start()
    {
        // 抓取身上的 SpriteRenderer (用來翻轉圖片)
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 記錄一開始的位置，這樣魚才不會越游越遠，游到畫面外
        startPosition = transform.position;
        
        // 遊戲一開始，先幫魚找一個隨機目標點
        PickNewTargetPosition();
    }

    void Update()
    {
        // 🌟 核心判定：如果被定身 (canMove 為 false) 或者正在休息 (isWaiting 為 true)，就不移動
        if (!canMove || isWaiting) return;

        // 1. 讓魚朝著「目標點」等速移動 (這行是 Unity 追蹤目標最標準的寫法)
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, fishSpeed * Time.deltaTime);

        // 2. 讓魚頭轉向移動的方向 (自動翻轉圖片)
        UpdateFacingDirection();

        // 3. 檢查是不是已經游到目標點了 (距離小於 0.1 就算到達)
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            // 游到了！開始執行休息的協程
            StartCoroutine(WaitAndPickNewPosition());
        }
    }

    // 隨機在地圖上找一個新座標
    private void PickNewTargetPosition()
    {
        // 在起點的周圍 (wanderRadius 半徑內) 隨機找一個 X 和 Y 座標
        float randomX = Random.Range(-wanderRadius, wanderRadius);
        float randomY = Random.Range(-wanderRadius, wanderRadius);
        
        // 設定新的目標點 (中心點 + 偏移量)
        targetPosition = startPosition + new Vector2(randomX, randomY);
    }

    // 休息倒數的碼表
    IEnumerator WaitAndPickNewPosition()
    {
        isWaiting = true; // 標記為正在休息，Update 裡的移動邏輯會暫停
        
        // 隨機決定要休息幾秒
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime); // 碼表開始計時，等待
        
        // 休息結束，找下一個目標點，並恢復移動
        PickNewTargetPosition();
        isWaiting = false; 
    }

    // 處理圖片左右翻轉的邏輯
    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null) return;

        // 如果目標點在魚的右邊
        if (targetPosition.x > transform.position.x)
        {
            // 如果原圖朝左，往右走就要翻轉 (true)
            spriteRenderer.flipX = !isFacingRightOriginally;
        }
        // 如果目標點在魚的左邊
        else if (targetPosition.x < transform.position.x)
        {
            // 往左走就恢復原狀
            spriteRenderer.flipX = isFacingRightOriginally;
        }
    }
}
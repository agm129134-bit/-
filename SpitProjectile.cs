using UnityEngine;

public class SpitProjectile : MonoBehaviour
{
    [Header("口水彈設定")]
    public float flySpeed = 10f; // 飛行速度
    public float stunDuration = 5f; // 困住人類的秒數
    public float lifeTime = 3f; // 自我銷毀時間

    // ==========================================
    // 🌟 【新增魔法】圖片原本的方向校正
    // ==========================================
    public enum SpriteForwardDirection { FacingRight, FacingLeft, FacingUp, FacingDown }
    
    [Header("視覺設定")]
    [Tooltip("這張圖片資產原本是『頭朝向哪邊』？ (例如水滴的 pointy end 朝哪)")]
    public SpriteForwardDirection imageForward = SpriteForwardDirection.FacingRight;

    // 內部變數
    private Vector2 fixedDirection; // 記錄被賦予的固定飛行方向
    private bool directionCalculated = false; // 標記方向是否已經算出

    void Start()
    {
        // 3秒後自動消失
        Destroy(gameObject, lifeTime);
    }

    // ==========================================
    // 讓大魚告訴口水彈要飛向哪裡，並處理圖片轉向
    // ==========================================
    public void SetDirection(Vector2 direction)
    {
        // 正規化 (把長度變為 1)，變成純方向
        fixedDirection = direction.normalized; 
        
        // ==========================================
        // 🌟 【超級修正】圖片轉向邏輯
        // ==========================================
        // 1. 算出飛行的絕對角度
        float angle = Mathf.Atan2(fixedDirection.y, fixedDirection.x) * Mathf.Rad2Deg;
        
        // 2. 算出圖片本身需要的偏移量
        // Unity 預設 2D 圖片正面是朝右邊 (Vector3.right)。
        float angleOffset = 0f;
        switch (imageForward)
        {
            case SpriteForwardDirection.FacingLeft: angleOffset = 180f; break; // 原圖朝左，要偏移 180 度指向右
            case SpriteForwardDirection.FacingUp:   angleOffset = -90f; break; // 原圖朝上，要偏移 -90 度指向右
            case SpriteForwardDirection.FacingDown: angleOffset = 90f; break; // 原圖朝下，要偏移 90 度指向右
            default: angleOffset = 0f; break; // 原圖朝右，不需要偏移
        }
        
        // 3. 將偏移量套用到角度上
        angle += angleOffset;
        
        // 4. 將角度套用到口水彈的旋轉上 (對準 Z 軸)
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        
        directionCalculated = true; // 標記為：方向已算出，可以飛了
        Debug.Log($"💦 口水彈收到指令，往方向 {fixedDirection} 飛出！圖片角度校正為: {angle} 度。");
    }

    void Update()
    {
        // 只有方向算出後才移動
        if (!directionCalculated) return;

        // 讓口水彈沿著「被賦予的固定方向」直線飛行。
        // 使用 Space.World 是為了直線飛，不會因為自身視覺旋轉而亂飛。
        transform.Translate(fixedDirection * flySpeed * Time.deltaTime, Space.World);
    }

    // ==========================================
    // 碰撞偵測 ( Is Trigger 必須打勾✅ )
    // ==========================================
    void OnTriggerEnter2D(Collider2D other)
    {
        // 認得人類 (人類標籤要設為 "Player")
        if (other.CompareTag("Player"))
        {
            // 抓出他身上的移動腳本，並呼叫定身功能
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.BeStunned(stunDuration);
            }

            // 命中後，口水彈銷毀
            Destroy(gameObject);
        }
    }
}
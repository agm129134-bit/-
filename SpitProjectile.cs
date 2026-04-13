using UnityEngine;

public class SpitProjectile : MonoBehaviour
{
    [Header("口水彈設定")]
    public float flySpeed = 10f; // 飛行速度
    public float stunDuration = 5f; // 困住人類的秒數
    public float lifeTime = 3f; // 自我銷毀時間

    // ==========================================
    // 🌟 命中/被控住的音效設定
    // ==========================================
    [Header("音效設定")]
    [Tooltip("請把打中人/被控住的音效拖進來 (現在會循環播放直到解除定身)")]
    public AudioClip hitSound;

    // ==========================================
    // 🌟 圖片原本的方向校正
    // ==========================================
    public enum SpriteForwardDirection { FacingRight, FacingLeft, FacingUp, FacingDown }
    
    [Header("視覺設定")]
    [Tooltip("這張圖片資產原本是『頭朝向哪邊』？ (例如水滴的 pointy end 朝哪)")]
    public SpriteForwardDirection imageForward = SpriteForwardDirection.FacingRight;

    // 內部變數
    private Vector2 fixedDirection; 
    private bool directionCalculated = false; 

    void Start()
    {
        // 3秒後自動消失
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 direction)
    {
        fixedDirection = direction.normalized; 
        
        float angle = Mathf.Atan2(fixedDirection.y, fixedDirection.x) * Mathf.Rad2Deg;
        
        float angleOffset = 0f;
        switch (imageForward)
        {
            case SpriteForwardDirection.FacingLeft: angleOffset = 180f; break; 
            case SpriteForwardDirection.FacingUp:   angleOffset = -90f; break; 
            case SpriteForwardDirection.FacingDown: angleOffset = 90f; break; 
            default: angleOffset = 0f; break; 
        }
        
        angle += angleOffset;
        
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        
        directionCalculated = true; 
        Debug.Log($"💦 口水彈收到指令，往方向 {fixedDirection} 飛出！圖片角度校正為: {angle} 度。");
    }

    void Update()
    {
        if (!directionCalculated) return;

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
            // ==========================================
            // 🌟 【全新進化】生成一個黏在玩家身上的「循環喇叭」
            // ==========================================
            if (hitSound != null)
            {
                // 1. 產生一個空的隱形物件來當喇叭
                GameObject loopSpeaker = new GameObject("StunAudioLoop");
                
                // 2. 把喇叭放在人類的位置，並設定為人類的子物件 (黏著他走)
                loopSpeaker.transform.position = other.transform.position;
                loopSpeaker.transform.SetParent(other.transform);
                
                // 3. 幫這個空物件加上 AudioSource 元件
                AudioSource audioSrc = loopSpeaker.AddComponent<AudioSource>();
                audioSrc.clip = hitSound;
                audioSrc.loop = true; // ✅ 開啟循環播放！
                audioSrc.Play(); // 開始播放
                
                // 4. 設定這個喇叭在 stunDuration (定身時間) 秒之後，自動銷毀關閉！
                Destroy(loopSpeaker, stunDuration);
            }

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
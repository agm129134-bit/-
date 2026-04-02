using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [Header("銷毀設定")]
    [Tooltip("幾秒後要自動銷毀這個物件？")]
    public float lifeTime = 1.5f;

    void Start()
    {
        // 核心魔法：Destroy 可以傳入第二個參數，代表「延遲幾秒後執行」
        // 這裡的意思是：在遊戲開始 lifeTime 秒後，摧毀自己 (gameObject)
        Destroy(gameObject, lifeTime);
    }
}
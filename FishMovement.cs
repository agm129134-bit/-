using UnityEngine;

// ==========================================
// 🐟 腳本 1：紅魚移動控制 (需新增凍結功能)
// 請將此腳本掛在地圖上的紅魚物件 ( image_13.png 中的魚) 身上
// ==========================================
public class FishMovement : MonoBehaviour
{
    // 魚的基礎移動速度，可以在 Unity 編輯器裡隨時調整
    public float fishSpeed = 3f;

    // 🌟 【核心魔法：凍結變數】
    // 預設為 true (可以移動)。當被設為 false 時，魚會停在原地。
    [HideInInspector] // 在 Inspector 中隱藏，避免手動修改
    public bool canMove = true;

    void Update()
    {
        // 🌟 核心判定：只有當 canMove 為 true 時才執行移動邏輯
        if (canMove)
        {
            // --- 這裡是你原本寫的魚移動邏輯，例如： ---
            // // 下面這行是基礎的往右移動測試：
            // transform.Translate(Vector2.right * fishSpeed * Time.deltaTime);
        }
    }
}
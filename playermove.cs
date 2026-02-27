using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 移動速度，可以在 Unity 編輯器裡隨時調整
    public float moveSpeed = 5f; 

    void Update()
    {
        // 1. 取得玩家輸入
        // GetAxisRaw 會在按下方向鍵 (或 WASD) 時回傳 -1, 0 或 1
        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveY = Input.GetAxisRaw("Vertical");   

        // 2. 將輸入轉換為方向向量
        // .normalized 可以確保斜向移動時，速度不會變成兩倍快
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // 3. 實際移動物件
        // transform.Translate 負責改變物件的座標
        // 乘以 Time.deltaTime 是為了確保在每一台效能不同的電腦上，移動速度都保持一致
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}
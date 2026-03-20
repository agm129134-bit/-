using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("這個道具在道具欄要顯示的圖案")]
    [Tooltip("把紅鞋子或相片的圖片拖進來")]
    public Sprite itemIconSprite; 

    private ItemBarManager itemManager;
    private bool isPlayerInRange = false; // 記錄玩家是不是站在道具旁邊

    void Start()
    {
        // 遊戲一開始，自動去地圖上尋找「道具管理員」報到
        itemManager = FindAnyObjectByType<ItemBarManager>();
    }

    void Update()
    {
        // 如果玩家在範圍內，而且按下了鍵盤的 F 鍵
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (itemManager != null)
            {
                // 告訴總管：玩家撿起我了，請把我放進道具欄！
                bool isSuccess = itemManager.AddItem(itemIconSprite);
                
                // 如果道具欄還沒滿（成功放進去了）
                if (isSuccess)
                {
                    Destroy(gameObject); // 摧毀地上的道具物件
                }
            }
        }
    }

    // 當有東西走進道具的綠色碰撞框時
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true; // 標記玩家已進入範圍
        }
    }

    // 當東西離開道具的綠色碰撞框時
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false; // 標記玩家已離開範圍
        }
    }
}
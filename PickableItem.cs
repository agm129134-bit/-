using UnityEngine;
using System.Collections.Generic;

public class PickableItem : MonoBehaviour
{
    [Header("這個道具在道具欄要顯示的圖案")]
    public Sprite itemIconSprite; 

    private ItemBarManager itemManager;
    private bool isPlayerInRange = false; 
    private Transform playerTransform; 

    public static List<PickableItem> nearbyItems = new List<PickableItem>();

    // 🌟 【超級防護罩】記錄「上一次按下 F 鍵並成功觸發」是哪一個畫面幀
    public static int lastInteractFrame = -1;

    void Start()
    {
        itemManager = FindAnyObjectByType<ItemBarManager>();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // 🛑 鎖頭防呆：如果同一個瞬間（同一幀），已經有其他人撿起東西或觸發機關了，我就乖乖閉嘴！
            if (lastInteractFrame == Time.frameCount) return;

            PickableItem closestItem = GetClosestItem();

            if (closestItem == this)
            {
                // 🌟 核心魔法：我搶到 F 鍵了！馬上把這一幀「上鎖」，其他人不准動！
                lastInteractFrame = Time.frameCount;

                if (itemManager != null)
                {
                    bool isSuccess = itemManager.AddItem(itemIconSprite);
                    if (isSuccess)
                    {
                        nearbyItems.Remove(this); 
                        Destroy(gameObject); 
                    }
                }
            }
        }
    }

    private PickableItem GetClosestItem()
    {
        PickableItem closest = null;
        float minDistance = float.MaxValue;
        
        nearbyItems.RemoveAll(item => item == null);

        foreach (PickableItem item in nearbyItems)
        {
            float distance = Vector2.Distance(playerTransform.position, item.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = item;
            }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerTransform = other.transform; 
            if (!nearbyItems.Contains(this)) nearbyItems.Add(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (nearbyItems.Contains(this)) nearbyItems.Remove(this);
        }
    }

    private void OnDestroy()
    {
        if (nearbyItems.Contains(this)) nearbyItems.Remove(this);
    }
}
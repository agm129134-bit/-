using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("道具模具 (預製體 Prefabs)")]
    [Tooltip("請把『布鞋 (加速)』的預製體拖進來")]
    public GameObject shoePrefab;
    [Tooltip("請把『時鐘 (加時)』的預製體拖進來")]
    public GameObject clockPrefab;

    [Header("生成數量設定")]
    [Tooltip("一場遊戲固定要產生幾個布鞋？ (預設 5)")]
    public int shoeCount = 5;
    
    [Tooltip("一場遊戲最少產生幾個時鐘？ (預設 0)")]
    public int minClockCount = 0;
    [Tooltip("一場遊戲最多產生幾個時鐘？ (預設 2)")]
    public int maxClockCount = 2;

    [Header("生成範圍設定 (不超出地圖草地邊界)")]
    [Tooltip("X 軸範圍 (最左邊, 最右邊)")]
    public Vector2 spawnRangeX = new Vector2(-800f, 800f); 
    [Tooltip("Y 軸範圍 (最下面, 最上面)")]
    public Vector2 spawnRangeY = new Vector2(-400f, 400f);

    void Start()
    {
        // 遊戲一開始就自動撒道具
        SpawnItems();
    }

    public void SpawnItems()
    {
        // 1. 生成布鞋
        for (int i = 0; i < shoeCount; i++)
        {
            SpawnSingleItem(shoePrefab);
        }

        // 2. 生成時鐘 (隨機決定數量)
        // 注意：Random.Range 的整數寫法，最大值要 +1 才會包含那個數字
        int clockCount = Random.Range(minClockCount, maxClockCount + 1);
        for (int i = 0; i < clockCount; i++)
        {
            SpawnSingleItem(clockPrefab);
        }
        
        Debug.Log($"✨ [道具生成] 地圖上隨機散落了 {shoeCount} 雙布鞋，與 {clockCount} 個時鐘！");
    }

    // 負責把單個道具「生」出來的魔法
    private void SpawnSingleItem(GameObject prefab)
    {
        if (prefab == null) return; // 防呆：如果忘記放模具就跳過

        // 隨機決定一個 XY 座標
        float randomX = Random.Range(spawnRangeX.x, spawnRangeX.y);
        float randomY = Random.Range(spawnRangeY.x, spawnRangeY.y);
        Vector2 spawnPosition = new Vector2(randomX, randomY);

        // 把道具生在該座標，並且把生成器當作它們的爸爸 (讓階層畫面比較乾淨)
        Instantiate(prefab, spawnPosition, Quaternion.identity, this.transform);
    }
}
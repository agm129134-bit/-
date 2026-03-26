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

    [Header("特殊道具生成 (照片觸發點)")]
    [Tooltip("請把 Project 視窗裡做好的『拼圖照片觸發點預製體』拖進來")]
    public GameObject puzzlePhotoPrefab;

    [Tooltip("照片要在湖的哪個位置生成？ (請在地圖中間湖的位置放一個空物件當參考點)")]
    public Transform lakeSpawnPoint; 

    void Start()
    {
        // 1. 撒地圖上的隨機道具 (布鞋、時鐘)
        SpawnItems();

        // 2. 單次生成湖中間的照片小遊戲觸發點
        SpawnPuzzlePhoto();
    }

    // ==========================================
    // 負責撒布鞋跟時鐘的函數
    // ==========================================
    public void SpawnItems()
    {
        // 1. 生成布鞋
        for (int i = 0; i < shoeCount; i++)
        {
            SpawnSingleItem(shoePrefab);
        }

        // 2. 生成時鐘 (隨機決定數量)
        int clockCount = Random.Range(minClockCount, maxClockCount + 1);
        for (int i = 0; i < clockCount; i++)
        {
            SpawnSingleItem(clockPrefab);
        }
        
        Debug.Log($"✨ [道具生成] 地圖上隨機散落了 {shoeCount} 雙布鞋，與 {clockCount} 個時鐘！");
    }

    // ==========================================
    // 負責把單個隨機道具「生」出來的魔法
    // ==========================================
    private void SpawnSingleItem(GameObject prefab)
    {
        if (prefab == null) return; 

        float randomX = Random.Range(spawnRangeX.x, spawnRangeX.y);
        float randomY = Random.Range(spawnRangeY.x, spawnRangeY.y);
        Vector2 spawnPosition = new Vector2(randomX, randomY);

        GameObject newItem = Instantiate(prefab, spawnPosition, Quaternion.identity, this.transform);

        // 🌟 【修正這裡】不要強制設為 1，而是讓它保留預製體 (Prefab) 本身設定好的大小！
        if (newItem != null)
        {
            newItem.transform.localScale = prefab.transform.localScale; 
        }
    }

    // ==========================================
    // 負責在湖中生成照片觸發點的魔法
    // ==========================================
    private void SpawnPuzzlePhoto()
    {
        if (puzzlePhotoPrefab == null || lakeSpawnPoint == null)
        {
            Debug.LogWarning("🚨 [道具生成] 忘記設定 Puzzle Photo Prefab 或 Lake Spawn Point 了，魚的照片沒生出來喔！");
            return;
        }

        // 直接生在指定參考點的位置
        GameObject newPhoto = Instantiate(puzzlePhotoPrefab, lakeSpawnPoint.position, Quaternion.identity, this.transform);

        // 🌟 【完美修正】讓生成的照片保留預製體 (Prefab) 本身設定好的大小 (也就是你的 0.2)
        if (newPhoto != null)
        {
            newPhoto.transform.localScale = puzzlePhotoPrefab.transform.localScale;
        }

        Debug.Log("✨ [道具生成] 在湖中成功生成了開啟拼圖小遊戲的魚照片！大小已完美同步！");
    }
}
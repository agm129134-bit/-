using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("道具模具 (預製體 Prefabs)")]
    public GameObject shoePrefab;
    public GameObject clockPrefab;

    [Header("生成數量設定")]
    public int shoeCount = 5;
    public int minClockCount = 0;
    public int maxClockCount = 2;

    [Header("生成範圍設定 (綠色大框框)")]
    public Vector2 spawnRangeX = new Vector2(-7f, 7f); 
    public Vector2 spawnRangeY = new Vector2(-3.5f, 3.5f);

    [Header("🌟 精準生成範圍 (不規則多邊形)")]
    public PolygonCollider2D spawnAreaPolygon;

    [Header("特殊道具設定")]
    public GameObject puzzlePhotoPrefab;
    public Transform lakeSpawnPoint; 

    [Header("防重疊與距離設定")]
    public float minSpawnDistance = 0.5f; 
    public float physicsCheckRadius = 0.5f; 
    public int maxSpawnAttempts = 150;

    private List<Vector2> occupiedPositions = new List<Vector2>();

    void Start()
    {
        occupiedPositions.Clear();
        if (lakeSpawnPoint != null)
        {
            occupiedPositions.Add(lakeSpawnPoint.position);
            SpawnPuzzlePhoto();
        }
        SpawnItems();
    }

    public void SpawnItems()
    {
        for (int i = 0; i < shoeCount; i++) SpawnSingleItem(shoePrefab);
        int clockCount = Random.Range(minClockCount, maxClockCount + 1);
        for (int i = 0; i < clockCount; i++) SpawnSingleItem(clockPrefab);
    }

    private void SpawnSingleItem(GameObject prefab)
    {
        if (prefab == null) return; 

        Vector2 finalSpawnPosition = Vector2.zero;
        bool positionFound = false;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float randomX = Random.Range(spawnRangeX.x, spawnRangeX.y);
            float randomY = Random.Range(spawnRangeY.x, spawnRangeY.y);
            Vector2 randomPos = new Vector2(randomX, randomY);

            // 1. 確保這個點在草地多邊形範圍內 (不會掉進水裡)
            if (spawnAreaPolygon != null && !spawnAreaPolygon.OverlapPoint(randomPos)) 
            {
                continue; 
            }

            bool isTooClose = false;

            // 2. 檢查跟其他生成的道具會不會太近
            foreach (Vector2 occupied in occupiedPositions)
            {
                if (Vector2.Distance(randomPos, occupied) < minSpawnDistance)
                {
                    isTooClose = true; 
                    break;             
                }
            }

            // 3. 🚨【終極修正】升級為「全範圍掃描」雷達
            if (!isTooClose)
            {
                // 取得這個圓圈內「所有」碰到的東西
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(randomPos, physicsCheckRadius);
                
                foreach (Collider2D hit in hitColliders)
                {
                    // 只要碰到的東西「不是草地」，就代表絕對撞到障礙物（垃圾袋、水池邊界等）！
                    if (hit != spawnAreaPolygon)
                    {
                        isTooClose = true;
                        break; // 確定撞到東西，馬上放棄這個點
                    }
                }
            }

            if (!isTooClose)
            {
                finalSpawnPosition = randomPos;
                positionFound = true;
                break; 
            }
        }

        if (positionFound)
        {
            GameObject newItem = Instantiate(prefab, finalSpawnPosition, Quaternion.identity, this.transform);
            if (newItem != null) newItem.transform.localScale = prefab.transform.localScale; 
            occupiedPositions.Add(finalSpawnPosition); 
        }
        else
        {
            Debug.LogWarning($"🚨 找不到空位生成 {prefab.name}！(可能綠色框框太小或安全距離要求太大)");
        }
    }

    private void SpawnPuzzlePhoto()
    {
        if (puzzlePhotoPrefab == null || lakeSpawnPoint == null) return;
        GameObject newPhoto = Instantiate(puzzlePhotoPrefab, lakeSpawnPoint.position, Quaternion.identity, this.transform);
        if (newPhoto != null) newPhoto.transform.localScale = puzzlePhotoPrefab.transform.localScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        float centerX = (spawnRangeX.x + spawnRangeX.y) / 2f;
        float centerY = (spawnRangeY.x + spawnRangeY.y) / 2f;
        Vector3 center = new Vector3(centerX, centerY, 0);

        float width = spawnRangeX.y - spawnRangeX.x;
        float height = spawnRangeY.y - spawnRangeY.x;
        Vector3 size = new Vector3(width, height, 0);

        Gizmos.DrawWireCube(center, size);
    }
}
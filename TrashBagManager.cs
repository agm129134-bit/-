using UnityEngine;

public class TrashBagManager : MonoBehaviour
{
    [Header("地圖上的所有垃圾袋位置")]
    public GameObject[] allTrashBags;

    [Header("每次遊戲要隨機出現幾個？")]
    public int bagsToSpawn = 3;

    void Start()
    {
        // 防呆機制：如果沒有設定垃圾袋，就不執行
        if (allTrashBags == null || allTrashBags.Length == 0) return;

        // 1. 遊戲一開始，先把地圖上所有的垃圾袋都「隱藏」起來
        for (int i = 0; i < allTrashBags.Length; i++)
        {
            if (allTrashBags[i] != null)
            {
                allTrashBags[i].SetActive(false);
            }
        }

        // 2. 熟悉的洗牌魔法：把垃圾袋名單的順序隨機打亂！
        for (int i = 0; i < allTrashBags.Length; i++)
        {
            int randomIndex = Random.Range(0, allTrashBags.Length);
            GameObject temp = allTrashBags[i];
            allTrashBags[i] = allTrashBags[randomIndex];
            allTrashBags[randomIndex] = temp;
        }

        // 3. 從洗好的名單中，挑選前 N 個垃圾袋，把它們「顯示」出來！
        // (Mathf.Min 是為了防止你輸入的數字比實際擺放的垃圾袋還要多而當機)
        int spawnCount = Mathf.Min(bagsToSpawn, allTrashBags.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            if (allTrashBags[i] != null)
            {
                allTrashBags[i].SetActive(true);
            }
        }
    }
}
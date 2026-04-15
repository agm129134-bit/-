using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    // 🌟 單例模式，方便大總管呼叫
    public static HealthUIManager Instance { get; private set; }

    // ==========================================
    // 🌐 【全新】連線多人專屬設定
    // ==========================================
    [Header("🌐 連線多人設定")]
    [Tooltip("測試用：設定這台電腦是幾 P？(0代表1P, 1代表2P, 2代表3P, 3代表4P)。\n未來接上連線套件後，這個值會由網路系統自動設定！")]
    public int localPlayerId = 0;

    [Header("❤️ 愛心圖片設定")]
    public Sprite fullHeartSprite;   // 紅心圖片
    public Sprite emptyHeartSprite;  // 灰心圖片
    
    [Tooltip("請製作一個 Image 當作預製體拖進來 (不需要掛腳本，純 Image 即可)")]
    public GameObject heartPrefab;   

    // 🌟 因為是連線，每個人畫面左上角都只有「自己」的血條
    [Header("UI 綁定")]
    [Tooltip("請把畫面左上角『唯一』的那個血條容器 (要有 HorizontalLayoutGroup) 拖進來")]
    public Transform localHeartContainer;

    // 內部陣列：只記錄「這台電腦」畫面上的愛心
    private Image[] myHearts;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 稍微延遲一下生成愛心，確保 GameManager 已經準備好
        Invoke("InitHealthUI", 0.1f);
    }

    // ==========================================
    // 🌟 自動生成「本機玩家」的專屬愛心
    // ==========================================
    private void InitHealthUI()
    {
        if (GameManager.Instance == null) return;
        if (localHeartContainer == null) return;

        int maxLives = GameManager.Instance.maxLivesPerPlayer;

        // 先清空容器裡原本的東西 (防呆)
        foreach (Transform child in localHeartContainer)
        {
            Destroy(child.gameObject);
        }

        myHearts = new Image[maxLives];

        // 只生成「自己」的血量，放在左上角
        for (int j = 0; j < maxLives; j++)
        {
            GameObject heartObj = Instantiate(heartPrefab, localHeartContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            heartImage.sprite = fullHeartSprite; // 初始設定為滿血紅心
            myHearts[j] = heartImage;
        }
    }

    // ==========================================
    // 💔 當有玩家受傷時，判斷是不是自己，是才更新畫面
    // ==========================================
    public void UpdateHealth(int playerId, int currentLives)
    {
        // 💡 核心連線邏輯：如果大總管廣播「有人受傷了」，但我發現受傷的不是「這台電腦的我」，那我就不理他！
        if (playerId != localPlayerId) return;
        
        if (myHearts == null) return;

        for (int i = 0; i < myHearts.Length; i++)
        {
            // 如果這個愛心的編號小於當前血量，它就是紅心；否則就是扣掉的灰心
            if (i < currentLives)
            {
                myHearts[i].sprite = fullHeartSprite;
            }
            else
            {
                myHearts[i].sprite = emptyHeartSprite;
            }
        }
    }
}
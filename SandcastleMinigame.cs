using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SandcastleMinigame : MonoBehaviour
{
    [Header("UI 與物件設定")]
    public GameObject minigameUI;         
    public GameObject[] checkmarks;              
    public SpriteRenderer mapSandcastleRenderer; 

    [Header("階段圖片設定")]
    public Image puddingDisplayUI;        
    
    [Header("新功能：大魚警告設定")]
    public Image whiteFrameUI;         // 白框的 UI 圖片 (也就是你的背景圖)
    public Transform playerTransform;  // 地圖上的玩家
    public Transform bigFishTransform; // 地圖上的大魚 (敵人)
    public float dangerDistance = 5f;  // 大魚靠近多近時框框會變紅？

    [System.Serializable]
    public struct SandcastleSet
    {
        public string castleName;        
        public Sprite[] buildingStages; 
        public Sprite finishedSprite;  
    }

    public SandcastleSet[] allCastleSets; 

    private bool isPlayerInRange = false;
    private bool isPlaying = false;
    private int clickCount = 0;
    private int currentSetIndex = 0; 
    private int completedCastles = 0; 

    private bool isAnimating = false;
    private RectTransform puddingRect;
    private Vector2 initialAnchoredPos; 

    void Start()
    {
        if (puddingDisplayUI != null)
        {
            puddingRect = puddingDisplayUI.GetComponent<RectTransform>();
            initialAnchoredPos = puddingRect.anchoredPosition;
        }
    }

    void Update()
    {
        if (isPlayerInRange && !isPlaying && Input.GetKeyDown(KeyCode.F))
        {
            StartNewGame();
        }

        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }

        // 【新功能】大魚靠近警告 (只有在玩小遊戲時才偵測)
        if (isPlaying && bigFishTransform != null && playerTransform != null && whiteFrameUI != null)
        {
            // 計算玩家跟大魚的距離
            float distance = Vector2.Distance(playerTransform.position, bigFishTransform.position);
            
            if (distance <= dangerDistance)
            {
                whiteFrameUI.color = Color.red; // 距離太近，框框變紅色！
            }
            else
            {
                whiteFrameUI.color = Color.white; // 安全距離，恢復原本的顏色
            }
        }
    }

    private void StartNewGame()
    {
        minigameUI.SetActive(true); 
        isPlaying = true;           
        completedCastles = 0; // 每次打開進度歸零
        isAnimating = false; 
        
        if (puddingDisplayUI != null && puddingRect != null)
        {
            puddingRect.anchoredPosition = initialAnchoredPos; 
        }

        foreach(var check in checkmarks)
        {
            if(check != null) check.SetActive(false);
        }

        if (whiteFrameUI != null)
        {
            whiteFrameUI.color = Color.white; // 打開時確保框框是正常的
        }
        
        PickRandomCastle();
    }

    // 【新功能】專門給 X 按鈕呼叫的關閉函數
    public void CloseAndResetMinigame()
    {
        if (isAnimating) return; // 動畫播放中先不准關

        isPlaying = false;                  
        minigameUI.SetActive(false);        
        completedCastles = 0; // 進度無情歸零
        
        // 把亮起的勾勾全部熄滅
        foreach(var check in checkmarks)
        {
            if(check != null) check.SetActive(false);
        }
        Debug.Log("未完成就關閉視窗，進度已重置！");
    }
    
    private void PickRandomCastle()
    {
        if (allCastleSets.Length > 0)
        {
            clickCount = 0; 
            currentSetIndex = Random.Range(0, allCastleSets.Length); 
            SetImageAlpha(0f); 
        }
    }

    private void HandleClick()
    {
        if (allCastleSets.Length == 0 || isAnimating) return;

        var currentSet = allCastleSets[currentSetIndex];
        
        if (clickCount == 0)
        {
            SetImageAlpha(1f);
            puddingDisplayUI.sprite = currentSet.buildingStages[0];
            clickCount++;
        }
        else if (clickCount < currentSet.buildingStages.Length)
        {
            puddingDisplayUI.sprite = currentSet.buildingStages[clickCount];
            clickCount++;
        }
        else 
        {
            StartCoroutine(SlideOutAndNext(currentSet));
        }
    }

    private IEnumerator SlideOutAndNext(SandcastleSet currentSet)
    {
        isAnimating = true; 

        puddingDisplayUI.sprite = currentSet.finishedSprite;

        float timer = 0f;
        float duration = 0.4f; 
        
        Vector2 startPos = puddingRect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(1500f, 0f); 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            puddingRect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            yield return null; 
        }

        SetImageAlpha(0f); 

        if (completedCastles < checkmarks.Length)
        {
            checkmarks[completedCastles].SetActive(true);
        }
        
        completedCastles++; 
        
        if (mapSandcastleRenderer != null)
        {
            mapSandcastleRenderer.gameObject.SetActive(true); 
            mapSandcastleRenderer.sprite = currentSet.finishedSprite; 
        }

        puddingRect.anchoredPosition = initialAnchoredPos;

        if (completedCastles >= 5)
        {
            FinishMinigame(); 
        }
        else
        {
            PickRandomCastle();
        }

        isAnimating = false; 
    }

    private void SetImageAlpha(float alphaValue)
    {
        if (puddingDisplayUI != null)
        {
            Color curColor = puddingDisplayUI.color; 
            curColor.a = alphaValue;                
            puddingDisplayUI.color = curColor;      
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            isPlaying = false;
            minigameUI.SetActive(false); 
        }
    }

    private void FinishMinigame()
    {
        isPlaying = false;                  
        minigameUI.SetActive(false);        
    }
}
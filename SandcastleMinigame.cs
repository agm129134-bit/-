using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SandcastleMinigame : MonoBehaviour
{
    [Header("UI 與物件設定")]
    public GameObject minigameUI;        
    public GameObject closeButton;       
    public GameObject[] checkmarks;              
    public SpriteRenderer mapSandcastleRenderer; 

    [Header("階段圖片設定")]
    public Image puddingDisplayUI;        
    
    [Header("大魚警告設定")]
    public Image whiteFrameUI;         
    public Transform playerTransform;  
    public Transform bigFishTransform; 
    public float dangerDistance = 5f;  

    [Header("音效設定 (Audio)")]
    public AudioSource audioSource;    
    public AudioClip clickSound;       
    public AudioClip castleDoneSound;  
    public AudioClip gameClearSound;   

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
            bool amIClosest = true;
            float myDistance = Vector2.Distance(playerTransform.position, transform.position);
            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(playerTransform.position, 2f);
            
            foreach (Collider2D obj in nearbyObjects)
            {
                if (obj.isTrigger && obj.gameObject != this.gameObject)
                {
                    float otherDistance = Vector2.Distance(playerTransform.position, obj.transform.position);
                    if (otherDistance < myDistance)
                    {
                        amIClosest = false;
                        break;
                    }
                }
            }

            if (amIClosest)
            {
                StartNewGame();
                Debug.Log("【除錯】按 F 鍵成功開啟遊戲！isPlaying 狀態切換為 true。");
            }
        }

        if (isPlaying && bigFishTransform != null && playerTransform != null && whiteFrameUI != null)
        {
            float distance = Vector2.Distance(playerTransform.position, bigFishTransform.position);
            whiteFrameUI.color = distance <= dangerDistance ? Color.red : Color.white;
        }

        // ==========================================
        // 🔎 探測器安裝在這裡！
        // ==========================================
        if (Input.GetMouseButtonDown(0))
        {
            if (!isPlaying)
            {
                Debug.Log("【除錯】你點了左鍵，但目前 isPlaying 是 false！(你是不是沒有按 F 開啟遊戲，而是直接把 UI 顯示在畫面上？)");
                return;
            }

            Debug.Log("【除錯】1. 系統偵測到左鍵點擊，且遊戲正在進行中。");

            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("【除錯】2. 滑鼠確實有點在 UI 上！");
                
                if (!IsPointerOverCloseButton())
                {
                    Debug.Log("【除錯】3. 點擊的不是叉叉，準備呼叫 HandleClick() 蓋沙堡...");
                    HandleClick();
                }
                else
                {
                    Debug.Log("【除錯】 -> 你點到了叉叉按鈕！(如果遊戲沒關閉，代表你的叉叉按鈕忘記在 Inspector 綁定 CloseAndResetMinigame 事件了)");
                }
            }
            else
            {
                Debug.Log("【除錯】 -> 系統覺得你【沒有】點到 UI。(請檢查 800x600 的背景 Image 裡面的 'Raycast Target' 有沒有打勾！)");
            }
        }
    }

    private bool IsPointerOverCloseButton()
    {
        if (closeButton == null) return false;
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == closeButton) return true; 
        }
        return false;
    }

    private void StartNewGame()
    {
        minigameUI.SetActive(true); 
        isPlaying = true;           
        completedCastles = 0; 
        isAnimating = false; 
        if (puddingDisplayUI != null && puddingRect != null) puddingRect.anchoredPosition = initialAnchoredPos; 
        foreach(var check in checkmarks) { if(check != null) check.SetActive(false); }
        if (whiteFrameUI != null) whiteFrameUI.color = Color.white; 
        PickRandomCastle();
    }

    public void CloseAndResetMinigame()
    {
        if (isAnimating) return; 
        isPlaying = false;                  
        if (minigameUI != null) minigameUI.SetActive(false);        
        completedCastles = 0; 
        foreach(var check in checkmarks) { if(check != null) check.SetActive(false); }
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

    public void HandleClick()
    {
        Debug.Log($"【除錯】4. 進入 HandleClick！你的沙堡圖庫數量有: {allCastleSets.Length} 個。isAnimating狀態: {isAnimating}");
        
        if (allCastleSets.Length == 0)
        {
            Debug.LogError("【警告】你的沙堡圖庫是 0！請到 Unity 的 Inspector 裡面把 All Castle Sets 的圖片設定加回去！");
            return;
        }
        
        if (isAnimating) return;

        var currentSet = allCastleSets[currentSetIndex];
        PlaySound(clickSound);

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
        PlaySound(castleDoneSound);
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
        if (completedCastles < checkmarks.Length) checkmarks[completedCastles].SetActive(true);
        completedCastles++; 
        if (mapSandcastleRenderer != null)
        {
            mapSandcastleRenderer.gameObject.SetActive(true); 
            mapSandcastleRenderer.sprite = currentSet.finishedSprite; 
        }
        puddingRect.anchoredPosition = initialAnchoredPos;
        if (completedCastles >= 5) FinishMinigame(); 
        else PickRandomCastle();
        isAnimating = false; 
    }

    private void FinishMinigame()
    {
        isPlaying = false;                  
        if (minigameUI != null) minigameUI.SetActive(false);  
        PlaySound(gameClearSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f); 
            audioSource.PlayOneShot(clip); 
        }
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

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) isPlayerInRange = true; }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            isPlaying = false;
            if (minigameUI != null) minigameUI.SetActive(false); 
        }
    }
}
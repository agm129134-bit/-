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
    }

    private void StartNewGame()
    {
        minigameUI.SetActive(true); 
        isPlaying = true;           
        completedCastles = 0; 
        isAnimating = false; 
        
        if (puddingDisplayUI != null && puddingRect != null)
        {
            puddingRect.anchoredPosition = initialAnchoredPos; 
        }

        foreach(var check in checkmarks)
        {
            if(check != null) check.SetActive(false);
        }
        
        PickRandomCastle();
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
            // 【已刪除 SetNativeSize】
            clickCount++;
        }
        else if (clickCount < currentSet.buildingStages.Length)
        {
            puddingDisplayUI.sprite = currentSet.buildingStages[clickCount];
            // 【已刪除 SetNativeSize】 
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
        // 【已刪除 SetNativeSize】

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
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 【新加入】負責偵測滑鼠有沒有點到 UI

public class SandcastleMinigame : MonoBehaviour
{
    [Header("UI 與物件設定")]
    public GameObject minigameUI;         
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
            StartNewGame();
        }

        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            // 【核心修正】如果滑鼠目前指著可互動的 UI (例如叉叉按鈕)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // 直接跳出，絕對不執行下面的堆沙堡動作！
            }

            HandleClick();
        }

        if (isPlaying && bigFishTransform != null && playerTransform != null && whiteFrameUI != null)
        {
            float distance = Vector2.Distance(playerTransform.position, bigFishTransform.position);
            
            if (distance <= dangerDistance)
            {
                whiteFrameUI.color = Color.red; 
            }
            else
            {
                whiteFrameUI.color = Color.white; 
            }
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

        if (whiteFrameUI != null)
        {
            whiteFrameUI.color = Color.white; 
        }
        
        PickRandomCastle();
    }

    public void CloseAndResetMinigame()
    {
        if (isAnimating) return; 

        isPlaying = false;                  
        minigameUI.SetActive(false);        
        completedCastles = 0; 
        
        foreach(var check in checkmarks)
        {
            if(check != null) check.SetActive(false);
        }
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

    private void FinishMinigame()
    {
        isPlaying = false;                  
        minigameUI.SetActive(false);  
        
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
}
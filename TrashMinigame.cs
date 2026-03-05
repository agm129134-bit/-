using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TrashMinigame : MonoBehaviour
{
    public enum TrashCategory { General, Plastic, Paper }

    [System.Serializable]
    public struct TrashData
    {
        public Sprite sprite;
        public Sprite nextImage;
        public TrashCategory category;
    }

    [Header("UI 與物件設定")]
    public GameObject minigameUI;
    public Canvas mainCanvas;
    public DraggableTrash currentTrashObj; 
    public Image nextTrashPreview;         
    public Vector2 trashSpawnPosition = new Vector2(0, -250f); 

    [Header("隱形判定區 (垃圾桶位置)")]
    public RectTransform generalBinTarget; 
    public RectTransform plasticBinTarget; 
    public RectTransform paperBinTarget;   
    public float snapDistance = 100f; 

    [Header("自訂垃圾庫與數量")]
    [Tooltip("這個垃圾袋要產生幾個垃圾？(現在可以超過題庫數量囉！)")]
    public int requiredTrashCount = 10; 
    
    [Tooltip("把你所有種類的垃圾放在這裡當作『總題庫』")]
    public TrashData[] allTrash;      

    // 系統會自動生成的「本局考卷清單」
    private TrashData[] currentSessionTrash;
    private int currentTrashIndex = 0;

    [Header("大魚警告設定")]
    public Image whiteFrameUI;
    public Transform playerTransform;
    public Transform bigFishTransform;
    public float dangerDistance = 5f;

    [Header("音效設定")]
    public AudioSource audioSource;
    public AudioClip clickSound;     
    public AudioClip correctSound;   
    public AudioClip wrongSound;     
    public AudioClip gameClearSound; 

    private bool isPlayerInRange = false;
    private bool isPlaying = false;

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
            }
        }

        if (isPlaying && bigFishTransform != null && playerTransform != null && whiteFrameUI != null)
        {
            float distance = Vector2.Distance(playerTransform.position, bigFishTransform.position);
            whiteFrameUI.color = distance <= dangerDistance ? Color.red : Color.white;
        }
    }

    private void StartNewGame()
    {
        // 防呆：如果題庫是空的就不要跑，免得當機
        if (allTrash == null || allTrash.Length == 0) return;

        minigameUI.SetActive(true);
        isPlaying = true;
        currentTrashIndex = 0; 

        if (currentTrashObj != null)
        {
            currentTrashObj.manager = this; 
        }

        if (whiteFrameUI != null) whiteFrameUI.color = Color.white;

        // ==========================================
        // 【全新抽考魔法】從題庫中隨機抽籤，製作一張全新的考卷！
        currentSessionTrash = new TrashData[requiredTrashCount];
        for (int i = 0; i < requiredTrashCount; i++)
        {
            int randomIndex = Random.Range(0, allTrash.Length);
            // 把抽到的垃圾放進本局考卷裡
            currentSessionTrash[i] = allTrash[randomIndex]; 
        }
        // ==========================================

        LoadTrash(); 
    }

    public void CloseAndResetMinigame()
    {
        isPlaying = false;
        minigameUI.SetActive(false);
        currentTrashIndex = 0;
    }

private void LoadTrash()
    {
        if (currentTrashIndex < requiredTrashCount)
        {
            currentTrashObj.GetComponent<Image>().sprite = currentSessionTrash[currentTrashIndex].sprite;
            currentTrashObj.ResetPosition(trashSpawnPosition); 
            currentTrashObj.gameObject.SetActive(true);

            if (currentTrashIndex + 1 < requiredTrashCount)
            {
                Sprite nextSp = currentSessionTrash[currentTrashIndex + 1].nextImage;
                if (nextSp == null) nextSp = currentSessionTrash[currentTrashIndex + 1].sprite;

                // 【空包彈警報器】如果雙重保險都找不到圖片，直接在控制台亮紅字大叫！
                if (nextSp == null)
                {
                    Debug.LogError("🚨 抓到空包彈了！下一個要出現的【" + currentSessionTrash[currentTrashIndex + 1].category + "】類垃圾完全沒有放圖片！快去檢查清單！");
                }

                nextTrashPreview.sprite = nextSp;
                
                if (nextTrashPreview.transform.parent != null)
                    nextTrashPreview.transform.parent.gameObject.SetActive(true);
                    
                nextTrashPreview.gameObject.SetActive(true);
            }
            else
            {
                if (nextTrashPreview.transform.parent != null)
                    nextTrashPreview.transform.parent.gameObject.SetActive(false);
                else
                    nextTrashPreview.gameObject.SetActive(false);
            }
        }
        else
        {
            FinishMinigame(); 
        }
    }

    public void CheckDrop(DraggableTrash trashObj, Vector2 dropPos, Vector2 startPos)
    {
        // 讀取本局考卷的答案
        TrashCategory requiredCategory = currentSessionTrash[currentTrashIndex].category;
        bool isCorrect = false;
        bool isDroppedInAnyBin = false;

        Vector2 trashPos = trashObj.transform.localPosition;
        Vector2 generalPos = generalBinTarget.localPosition;
        Vector2 plasticPos = plasticBinTarget.localPosition;
        Vector2 paperPos = paperBinTarget.localPosition;

        if (Vector2.Distance(trashPos, generalPos) <= snapDistance)
        {
            isDroppedInAnyBin = true;
            if (requiredCategory == TrashCategory.General) isCorrect = true;
        }
        else if (Vector2.Distance(trashPos, plasticPos) <= snapDistance)
        {
            isDroppedInAnyBin = true;
            if (requiredCategory == TrashCategory.Plastic) isCorrect = true;
        }
        else if (Vector2.Distance(trashPos, paperPos) <= snapDistance)
        {
            isDroppedInAnyBin = true;
            if (requiredCategory == TrashCategory.Paper) isCorrect = true;
        }

        if (isCorrect)
        {
            PlaySound(correctSound);
            currentTrashIndex++; 
            LoadTrash();         
        }
        else if (isDroppedInAnyBin)
        {
            PlaySound(wrongSound); 
            trashObj.ResetPosition(startPos); 
        }
        else
        {
            trashObj.ResetPosition(startPos); 
        }
    }

    private void FinishMinigame()
    {
        isPlaying = false;
        if (currentTrashObj != null) currentTrashObj.gameObject.SetActive(false);
        PlaySound(gameClearSound);
        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        if (minigameUI != null) minigameUI.SetActive(false);
        gameObject.SetActive(false); 
    }

    public void PlayClickSound()
    {
        PlaySound(clickSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (isPlaying) CloseAndResetMinigame();
        }
    }
}
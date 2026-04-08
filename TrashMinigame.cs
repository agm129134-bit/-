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
    public AudioClip correctSound;   // 丟進正確垃圾桶的音效
    public AudioClip wrongSound;     // 丟進錯誤垃圾桶的音效
    public AudioClip gameClearSound; 

    private bool isPlayerInRange = false;
    private bool isPlaying = false;

    void Update()
    {
        if (isPlayerInRange && !isPlaying && Input.GetKeyDown(KeyCode.F))
        {
            // 🛑 【第一道鎖：檢查紅綠燈】
            // 如果這一瞬間已經有其他道具被撿起或小遊戲被開啟，我立刻退下！
            if (PickableItem.lastInteractFrame == Time.frameCount) return;

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
                // 🌟 【第二道鎖：搶下紅綠燈】
                // 確定我是離玩家最近的垃圾堆了！馬上把燈按亮，不准其他腳本搶 F 鍵！
                PickableItem.lastInteractFrame = Time.frameCount;

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
        if (allTrash == null || allTrash.Length == 0) return;

        minigameUI.SetActive(true);
        isPlaying = true;
        currentTrashIndex = 0; 

        if (currentTrashObj != null)
        {
            currentTrashObj.manager = this; 
        }

        if (whiteFrameUI != null) whiteFrameUI.color = Color.white;

        currentSessionTrash = new TrashData[requiredTrashCount];
        for (int i = 0; i < requiredTrashCount; i++)
        {
            int randomIndex = Random.Range(0, allTrash.Length);
            currentSessionTrash[i] = allTrash[randomIndex]; 
        }

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
            // 單純換圖片就好，不要用程式碼強制改變它的大小
            currentTrashObj.GetComponent<Image>().sprite = currentSessionTrash[currentTrashIndex].sprite;
            currentTrashObj.ResetPosition(trashSpawnPosition); 
            currentTrashObj.gameObject.SetActive(true);

            if (currentTrashIndex + 1 < requiredTrashCount)
            {
                Sprite nextSp = currentSessionTrash[currentTrashIndex + 1].nextImage;
                if (nextSp == null) nextSp = currentSessionTrash[currentTrashIndex + 1].sprite;

                if (nextSp == null)
                {
                    Debug.LogError("🚨 抓到空包彈了！下一個要出現的【" + currentSessionTrash[currentTrashIndex + 1].category + "】類垃圾完全沒有放圖片！快去檢查清單！");
                }

                // 預告區也是單純換圖片就好
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

        // 🎵 音效觸發區
        if (isCorrect)
        {
            PlaySound(correctSound); // 答對音效
            currentTrashIndex++; 
            LoadTrash();         
        }
        else if (isDroppedInAnyBin)
        {
            PlaySound(wrongSound);   // 答錯音效
            trashObj.ResetPosition(startPos); 
        }
        else
        {
            // 如果丟在外面（沒丟進垃圾桶），就不發出聲音，直接彈回原位
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

    // 終極防錯播放器：完全不用在 Inspector 拉喇叭了！
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            // 1. 每次需要聲音時，直接在場景裡召喚一個「隱形播音員」
            GameObject soundPlayer = new GameObject("TempSoundPlayer");
            AudioSource tempSource = soundPlayer.AddComponent<AudioSource>();

            // 2. 把音效檔交給他，並設定隨機音高
            tempSource.clip = clip;
            tempSource.pitch = Random.Range(0.9f, 1.1f);
            
            // 3. ✨ 最重要的一步：強制設為 2D 音效 (0)，保證無論在哪裡都超級大聲清楚！
            tempSource.spatialBlend = 0f; 
            tempSource.volume = 1f;

            // 4. 開始播放
            tempSource.Play();

            // 5. 播完之後，這個隱形播音員就會自動消失，完全不佔用效能
            Destroy(soundPlayer, clip.length + 0.1f);
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
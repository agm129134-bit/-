using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("拼圖設定")]
    public RectTransform targetSlot; 
    public PuzzleMinigame manager;   
    public float snapDistance = 50f; 

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private bool isLocked = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        // 【已刪除】原本在這裡死背位子的程式碼被我們拿掉了
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        manager.PlayClickSound(); 
        canvasGroup.blocksRaycasts = false; 
        canvasGroup.alpha = 0.8f;           
        transform.SetAsLastSibling();       
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        rectTransform.anchoredPosition += eventData.delta / manager.mainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.blocksRaycasts = true; 
        canvasGroup.alpha = 1f;            

        float distance = Vector2.Distance(rectTransform.anchoredPosition, targetSlot.anchoredPosition);

        if (distance <= snapDistance)
        {
            rectTransform.anchoredPosition = targetSlot.anchoredPosition;
            isLocked = true; 
            manager.PieceLocked(); 
        }
    }

    // 【修改重點】現在改由大總管發配一個「隨機新位置 (newStartPos)」給它
    public void ResetPiece(Vector2 newStartPos)
    {
        rectTransform.anchoredPosition = newStartPos; 
        isLocked = false; 
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
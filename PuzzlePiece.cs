using UnityEngine;
using UnityEngine.EventSystems; // 這是 UI 拖曳魔法的關鍵

// 自動幫你加上 CanvasGroup，用來控制透明度和滑鼠穿透
[RequireComponent(typeof(CanvasGroup))]
public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("拼圖設定")]
    public RectTransform targetSlot; // 這塊拼圖正確的隱形目標位置
    public PuzzleMinigame manager;   // 遊戲大總管
    public float snapDistance = 50f; // 靠近多近會自動「吸附」進去？

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPos;
    private bool isLocked = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        // 記住遊戲剛打開時，拼圖散落在下方的位置
        originalPos = rectTransform.anchoredPosition; 
    }

    // 當滑鼠「剛按下去」開始拖的時候
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return; // 如果已經拼對了，就鎖死不准再拉

        manager.PlayClickSound(); // 呼叫總管播「拿起」的音效
        canvasGroup.blocksRaycasts = false; // 讓滑鼠能穿透這張圖，才抓得到底下的東西
        canvasGroup.alpha = 0.8f;           // 拿起時變半透明，增加手感
        transform.SetAsLastSibling();       // 把這塊圖層移到最上層，才不會被別人擋住
    }

    // 當滑鼠「拖曳中」的時候
    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        // 讓圖片跟著滑鼠移動 (除以 scaleFactor 確保不管螢幕多大，拖曳速度都正確)
        rectTransform.anchoredPosition += eventData.delta / manager.mainCanvas.scaleFactor;
    }

    // 當滑鼠「放開」的時候
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.blocksRaycasts = true; // 恢復滑鼠阻擋
        canvasGroup.alpha = 1f;            // 恢復不透明

        // 計算現在位置，距離隱形目標有多遠？
        float distance = Vector2.Distance(rectTransform.anchoredPosition, targetSlot.anchoredPosition);

        if (distance <= snapDistance)
        {
            // 距離夠近！直接完美吸附到目標點！
            rectTransform.anchoredPosition = targetSlot.anchoredPosition;
            isLocked = true; // 鎖定
            manager.PieceLocked(); // 告訴大總管：「我拼好一塊了！」
        }
    }

    // 給大總管用的重置功能
    public void ResetPiece()
    {
        rectTransform.anchoredPosition = originalPos; // 瞬間飛回底部的散落位置
        isLocked = false; 
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
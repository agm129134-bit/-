using UnityEngine;
using UnityEngine.UI;

// ==========================================
// 🖼️ 腳本 2：照片全螢幕覆蓋管理
// 請建立一個空的 GameObject 命名為 'PhotoOverlayManager' 並掛上此腳本
// ==========================================
public class PhotoOverlayManager : MonoBehaviour
{
    [Header("UI 設定")]
    [Tooltip("請把 Canvas 底下的全螢幕照片 UI Image 物件拖進來")]
    public Image photoOverlayImage;

    [Header("照片設定")]
    [Tooltip("請把美人魚照片圖片 (image_21.png) 拖進來")]
    public Sprite mermaidPhotoSprite;

    void Start()
    {
        // 遊戲一開始，確認照片是隱藏的，且 Sprite 是正確的
        if (photoOverlayImage != null)
        {
            photoOverlayImage.sprite = mermaidPhotoSprite;
            
            // 💡 為了讓照片能完全佔據全螢幕，請確保 Unity Inspector 裡
            // Rect Transform 的錨點設定為全螢幕 (Top Left, Stretch Stretch)
            
            photoOverlayImage.gameObject.SetActive(false); // 預設隱藏
        }
    }

    // ==========================================
    // ✨ 功能按鈕：顯示全螢幕照片
    // ==========================================
    public void ShowPhoto()
    {
        if (photoOverlayImage != null)
        {
            photoOverlayImage.gameObject.SetActive(true);
            
            // 可以選擇性地把道具欄 UI 隱藏，讓畫面更乾淨
            // CanvasGroup cgItemBar = itemBarManager.GetComponent<CanvasGroup>();
            // if (cgItemBar != null) cgItemBar.alpha = 0f;
        }
    }

    // ==========================================
    // ✨ 功能按鈕：隱藏全螢幕照片
    // ==========================================
    public void HidePhoto()
    {
        if (photoOverlayImage != null)
        {
            photoOverlayImage.gameObject.SetActive(false);
            
            // 恢復道具欄 UI
            // CanvasGroup cgItemBar = itemBarManager.GetComponent<CanvasGroup>();
            // if (cgItemBar != null) cgItemBar.alpha = 1f;
        }
    }
}
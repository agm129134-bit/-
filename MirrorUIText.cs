using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MirrorUIText : MonoBehaviour
{
    [Header("同步設定")]
    public Text sourceText;
    private Text myText;

    void Start()
    {
        myText = GetComponent<Text>();
        
        // 遊戲一開始先自我檢查
        if (sourceText == null) Debug.LogError("❌ 鏡子腳本回報：老闆，你忘記綁定來源 Text 了！");
        if (myText == null) Debug.LogError("❌ 鏡子腳本回報：我身上沒有 Text 元件啊！");
    }

    void Update()
    {
        if (sourceText != null && myText != null)
        {
            // 🌟 抓蟲魔法：每 60 個影格 (大約1秒)，在 Console 印出它看到的東西
            if (Time.frameCount % 60 == 0) 
            {
                Debug.Log($"🪞 鏡子回報 ➔ 我看到人類時間是：[{sourceText.text}]，大魚時間是：[{myText.text}]");
            }

            // 同步文字
            if (myText.text != sourceText.text)
            {
                myText.text = sourceText.text;
            }
        }
    }
}
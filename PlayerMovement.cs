using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))] 
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    private float originalSpeed;

    [Header("狀態監控 (除錯用)")]
    public bool isStunned = false; 

    private SpriteRenderer sr;
    private Animator myAnimator;
    private Coroutine stunCoroutine;
    private Coroutine boostCoroutine; 

    private enum Facing { Left, Right, Front }
    private Facing currentFacing = Facing.Left; // 預設原圖朝左

    // 🌟 變乾淨了！我們只需要 左邊、前面、結束 這三個開關！
    private const string ANIM_PARA_STUN_LEFT = "Stun_Left";   
    private const string ANIM_PARA_STUN_FRONT = "Stun_Front"; 
    private const string ANIM_PARA_END_STUN = "EndStun";

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        myAnimator = GetComponent<Animator>();
        originalSpeed = moveSpeed;
    }

    void Update()
    {
        if (isStunned) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveX, moveY).normalized;
        transform.Translate(movement * moveSpeed * Time.deltaTime);

        // 自動判斷面向並翻轉圖片
        if (moveX > 0.1f)
        {
            sr.flipX = true; // 往右走：圖片水平翻轉！
            currentFacing = Facing.Right;
        }
        else if (moveX < -0.1f)
        {
            sr.flipX = false; // 往左走：維持原圖
            currentFacing = Facing.Left;
        }
        else if (moveY > 0.1f || moveY < -0.1f)
        {
            currentFacing = Facing.Front;
        }
    }

    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        if (boostCoroutine != null) StopCoroutine(boostCoroutine);
        boostCoroutine = StartCoroutine(BoostRoutine(multiplier, duration));
    }

    IEnumerator BoostRoutine(float multiplier, float duration)
    {
        moveSpeed = originalSpeed * multiplier; 
        yield return new WaitForSeconds(duration); 
        moveSpeed = originalSpeed; 
        boostCoroutine = null;
    }

    public void BeStunned(float duration)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true; 

        if (myAnimator != null)
        {
            if (currentFacing == Facing.Right)
            {
                // 🌟 偷懶魔法：面向右邊時，直接呼叫「左邊」的動畫開關！
                // 因為圖片已經被翻轉了，播出來自然就是朝右的！
                myAnimator.SetTrigger(ANIM_PARA_STUN_LEFT);
            }
            else if (currentFacing == Facing.Left)
            {
                myAnimator.SetTrigger(ANIM_PARA_STUN_LEFT);
            }
            else if (currentFacing == Facing.Front)
            {
                myAnimator.SetTrigger(ANIM_PARA_STUN_FRONT);
            }
        }

        yield return new WaitForSeconds(duration);

        if (myAnimator != null)
        {
            myAnimator.SetTrigger(ANIM_PARA_END_STUN); 
        }

        isStunned = false; 
    }
}
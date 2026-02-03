using UnityEngine;

public class HandAnimationManager : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private UISpriteAnimator leftHandAnimator;
    [SerializeField] private UISpriteAnimator rightHandAnimator;

    [Header("Animation")]
    [SerializeField] private UISpriteAnimation reachAnimation;

    [Header("Walk Animation Settings")]
    [SerializeField] private float walkBounceHeight = 10f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float returnToDefaultSpeed = 5f;

    private const float DEFAULT_ANCHOR_Y = 0f;

    private RectTransform leftHandRect;
    private RectTransform rightHandRect;
    private bool isWalking;
    private float walkTimer;

    private void Awake()
    {
        if (leftHandAnimator != null)
            leftHandRect = leftHandAnimator.GetComponent<RectTransform>();

        if (rightHandAnimator != null)
            rightHandRect = rightHandAnimator.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (isWalking)
        {
            AnimateWalk();
        }
        else
        {
            ReturnToDefaultPosition();
        }
    }

    public void StartWalking()
    {
        isWalking = true;
        walkTimer = 0f;
    }

    public void StopWalking()
    {
        isWalking = false;
    }

    public void PlayReachAnimation()
    {
        if (reachAnimation == null)
        {
            Debug.LogWarning("HandAnimationManager: ReachAnimation not assigned!");
            return;
        }

        if (leftHandAnimator != null)
            leftHandAnimator.Play(reachAnimation);

        if (rightHandAnimator != null)
            rightHandAnimator.Play(reachAnimation);
    }

    private void AnimateWalk()
    {
        walkTimer += Time.deltaTime * walkSpeed;
        float bounceOffset = Mathf.Sin(walkTimer) * walkBounceHeight;

        if (leftHandRect != null)
        {
            Vector2 anchoredPos = leftHandRect.anchoredPosition;
            anchoredPos.y = bounceOffset;
            leftHandRect.anchoredPosition = anchoredPos;
        }

        if (rightHandRect != null)
        {
            Vector2 anchoredPos = rightHandRect.anchoredPosition;
            anchoredPos.y = bounceOffset;
            rightHandRect.anchoredPosition = anchoredPos;
        }
    }

    private void ReturnToDefaultPosition()
    {
        bool leftReturned = MoveTowardDefaultY(leftHandRect);
        bool rightReturned = MoveTowardDefaultY(rightHandRect);
    }

    private bool MoveTowardDefaultY(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return true;

        Vector2 anchoredPos = rectTransform.anchoredPosition;

        if (Mathf.Approximately(anchoredPos.y, DEFAULT_ANCHOR_Y))
            return true;

        anchoredPos.y = Mathf.Lerp(anchoredPos.y, DEFAULT_ANCHOR_Y, Time.deltaTime * returnToDefaultSpeed);

        if (Mathf.Abs(anchoredPos.y - DEFAULT_ANCHOR_Y) < 0.01f)
            anchoredPos.y = DEFAULT_ANCHOR_Y;

        rectTransform.anchoredPosition = anchoredPos;

        return Mathf.Approximately(anchoredPos.y, DEFAULT_ANCHOR_Y);
    }
}

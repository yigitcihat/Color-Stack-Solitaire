using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum HoverDirection { Vertical, Horizontal }

public class HoverAnimation : MonoBehaviour
{
    public HoverDirection HoverDirection = HoverDirection.Vertical;
    public Ease Ease = Ease.Linear;
    public float Duration = 1f;
    public float HoverDistance = 100f;

    RectTransform hoverTransform;
    float animationInitialPosX;
    float animationInitialPosY;

    private void Start()
    {
        hoverTransform = GetComponent<RectTransform>();

        switch (HoverDirection)
        {
            case HoverDirection.Vertical:
                VerticalMovement();
                break;
            case HoverDirection.Horizontal:
                HorizontalMovement();
                break;
            default:

                break;
        }
    }

    void HorizontalMovement()
    {
        transform.DOMoveX(transform.localPosition.x + HoverDistance * 2f, Duration)
            .SetEase(Ease)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void VerticalMovement()
    {
        transform.DOMoveY(transform.localPosition.y + HoverDistance * 2f, Duration)
            .SetEase(Ease)
            .SetLoops(-1, LoopType.Yoyo);
    }
}

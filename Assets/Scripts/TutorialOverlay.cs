using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialOverlay : MonoBehaviour
{
    private RectTransform rectTransform;
    private TextMeshProUGUI tutorialText;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        tutorialText = GetComponentInChildren<TextMeshProUGUI>();
        if (GameManager.level >1)
            Destroy(rectTransform.gameObject);
        rectTransform.anchoredPosition = new Vector2(0, Screen.height + rectTransform.rect.height);
       
    }

    internal void SetText(string text)
    {
        tutorialText.text = text;
    }
    internal void MoveDown()
    {
        rectTransform.DOAnchorPos(new Vector2(0,137),GameManager.animationDuration).SetEase(Ease.OutSine);
    }
    internal void MoveUp()
    {
        rectTransform.DOAnchorPos(new Vector2(0, Screen.height + rectTransform.rect.height),GameManager.animationDuration * 2).SetEase(Ease.InSine);
        
    }
}

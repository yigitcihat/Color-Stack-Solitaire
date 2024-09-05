using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{ 
    private static GameManager manager => GameManager.Instance;
    
    [SerializeField] private Transform startPanel, playButton, shuffleButton, hintButton, restartButton, StartButton,settingsButton;
    [SerializeField] private Image overlay, icon;
    [SerializeField] internal GameObject windowsManager;
    [SerializeField] internal TextMeshProUGUI levelLabel;

    private void Start()
    {
        if (GameManager.level > 1)
        {
            ExitCircle();
            return;
        }
        
        overlay.transform.localScale = Vector3.zero;

        shuffleButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        playButton.transform.DOLocalMoveY(playButton.transform.localPosition.y + 750, GameManager.animationDuration).SetEase(Ease.OutBack).SetDelay(GameManager.animationDuration);
        icon.DOFade(1, GameManager.animationDuration).SetEase(Ease.OutSine);
    }
    
    private void EnterCircle() => overlay.transform.DOScale(Vector3.one * 30, GameManager.animationDuration).SetEase(Ease.OutSine).OnComplete(() => { SceneManager.LoadScene(0); });

    private void ExitCircle() => overlay.transform.DOScale(0, GameManager.animationDuration / 1.5f).SetEase(Ease.InSine);

    public void TappedShuffle()
    {
        if (PowerUp.shuffle.count < 1) return;
        if (GameManager.Instance.isAnimating) return;
        PowerUp.shuffle.Use();
        Taptic.Medium();
        GameManager.Instance.ShuffleAndDeal();
    }

    public void TappedHint()
    {
        if (PowerUp.hint.count < 1) return;
        if (manager.isHintActive) return;
        Taptic.Medium();
        manager.ActivateHint(false);
    }

    public void TappedNextLevel()
    {
        Taptic.Medium();
        SaveManager.Clear();
        EnterCircle();
    }

    public void TappedPlay()
    {
        Taptic.Medium();
        var canvasGroup = startPanel.GetComponent<CanvasGroup>();
        
        canvasGroup.DOFade(0, GameManager.animationDuration * 3);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        playButton.transform.DOLocalMoveY(playButton.transform.localPosition.y - 750, GameManager.animationDuration).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                if (GameManager.level == 1)
                {
                    EnterTutorial();
                    return;
                }

                GameManager.Instance.CreateCards();
            });
    }

    public void TappedSettings()
    {
        Window.settings.Enter();
    }

    public void TappedStart()
    {
        if (GameManager.level == 1) manager.tutorialPopUp.MoveUp();
        Taptic.Medium();
        GameManager.Instance.CreateCards();
        StartButton.transform.DOLocalMoveY(StartButton.transform.localPosition.y - 1200, GameManager.animationDuration).SetEase(Ease.OutBack);
    }

    private void EnterTutorial()
    {
        GameManager.Instance.tutorialPopUp.MoveDown();
        StartButton.transform.DOLocalMoveY(StartButton.transform.localPosition.y + 600, GameManager.animationDuration).SetEase(Ease.OutBack).SetDelay(GameManager.animationDuration * 6);
    }
}
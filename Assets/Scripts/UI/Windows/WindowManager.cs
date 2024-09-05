using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WindowManager: Singleton<WindowManager>
{
    [SerializeField] internal RectTransform winWindow, loseWindow, shuffleWindow, settingsWindow;
    [SerializeField] private Image overlay;

    public void Enter(int window)
    {
        overlay.DOFade(0.7f, GameManager.animationDuration);
        View(window).DOAnchorPosY(0, GameManager.animationDuration);
    }
    
    public void Exit(int window)
    {
        overlay.DOFade(0, GameManager.animationDuration);
        View(window).DOAnchorPosY(Screen.height + View(window).rect.height, GameManager.animationDuration).OnComplete(() => UIManager.Instance.windowsManager.SetActive(false));
    }
    
    private RectTransform View(int window) => (Windows) window switch
    {
        Windows.win => WindowManager.Instance.winWindow,
        Windows.lose => WindowManager.Instance.loseWindow,
        Windows.shuffle => WindowManager.Instance.shuffleWindow,
        Windows.settings => WindowManager.Instance.settingsWindow,
        _ => WindowManager.Instance.settingsWindow
    };
}

[Serializable]
public enum Windows { win, lose, shuffle, settings }

[Serializable]
public class Window
{
    public static readonly Window win = new Window(Windows.win);
    public static readonly Window lose = new Window(Windows.lose);
    public static readonly Window shuffle = new Window(Windows.shuffle);
    public static readonly Window settings = new Window(Windows.settings);
    
    private readonly Windows window;

    private Window(Windows window) => this.window = window;

    public RectTransform View() => window switch
    {
        Windows.win => WindowManager.Instance.winWindow,
        Windows.lose => WindowManager.Instance.loseWindow,
        Windows.shuffle => WindowManager.Instance.shuffleWindow,
        Windows.settings => WindowManager.Instance.settingsWindow,
        _ => WindowManager.Instance.settingsWindow
    };

    public void Enter()
    {
        UIManager.Instance.windowsManager.SetActive(true);
        WindowManager.Instance.Enter((int) window);
    }
    
    public void Exit() => WindowManager.Instance.Exit((int) window);
}
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : Singleton<SettingsManager>
{
    private const int ToggleOnPos = 44;
    private const int ToggleOffPos = -44;
    
    [SerializeField] private Button hapticButton, sfxButton, backButton;
    [SerializeField] private RectTransform hapticToggle, sfxToggle;

    private Button button(Settings setting) => setting switch
    {
        Settings.sound => sfxButton,
        _ => hapticButton
    };
    
    private RectTransform rect(Settings setting) => setting switch
    {
        Settings.sound => sfxToggle,
        _ => hapticToggle
    };

    private void Awake()
    {
        Setting.allCases.ForEach(s => rect(s.setting).DOAnchorPosX(s.isOn ? ToggleOnPos : ToggleOffPos, 0));
    }

    public void Tapped(int setting) => ToggleMove(new Setting((Settings) setting));

    public void TappedDismiss() => Window.settings.Exit();
    
    private void ToggleMove(Setting setting)
    {
        setting.Toggle();
        var toggle = rect(setting.setting);
        var xValue = toggle.anchoredPosition.x;
        toggle.DOAnchorPosX(44 * (Mathf.Approximately(xValue, 44) ? -1 : 1), 0.3f);
    }
}

public enum Settings { sound, haptics }

internal class Setting {
    internal static readonly Setting sound = new (Settings.sound);
    private static readonly Setting haptics = new (Settings.haptics);
    internal readonly Settings setting;
    internal static readonly List<Setting> allCases = new() { sound, haptics };
    private string name => Enum.GetName(typeof(Settings), setting);

    private bool state
    {
        get => PlayerPrefs.GetInt(name, 1) == 1;
        set => PlayerPrefs.SetInt(name, value ? 1 : 0);
    }

    internal Setting(Settings setting) => this.setting = setting;

    internal bool isOn => state;
    internal void Toggle() => Set(!isOn);

    private void Set(bool newState)
    {
        if (setting == Settings.haptics) Taptic.tapticOn = newState;
        state = newState;
    }
}
